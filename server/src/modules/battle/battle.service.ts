import { randomUUID } from 'crypto'

import type { Pool } from 'pg'

import {
  resolveBattle,
  generateEnemiesForWave,
  createSeed,
  type CombatStats,
  type BattleOutcome
} from './combat-engine'
import type {
  BattleRecord,
  StartBattleRequest,
  StartBattleResponse
} from './battle.schema'

const DEFAULT_THRALL_STATS: CombatStats = {
  maxHealth: 150,
  attack: 25,
  defense: 10,
  speed: 1.2
}

const BASE_ENEMY_STATS: CombatStats = {
  maxHealth: 90,
  attack: 14,
  defense: 4,
  speed: 1.0
}

function mulberry32(seed: number): () => number {
  return () => {
    let t = (seed += 0x6d2b79f5)
    t = Math.imul(t ^ (t >>> 15), t | 1)
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61)
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296
  }
}

function calculateRewards(
  wave: number,
  result: BattleOutcome['result']
): { duskenCoin: number; bloodShards: number; xp: number } {
  const baseReward = 10 + wave * 5
  const killBonus = result.enemiesKilled * 2

  let duskenCoin = baseReward + killBonus
  let bloodShards = 0
  let xp = wave * 10 + result.enemiesKilled * 5

  if (result.winner === 'player') {
    duskenCoin = Math.round(duskenCoin * 1.5)
    xp = Math.round(xp * 1.5)

    if (wave % 10 === 0) {
      bloodShards = Math.floor(wave / 10)
    }
  } else if (result.winner === 'enemy') {
    duskenCoin = Math.round(duskenCoin * 0.3)
    xp = Math.round(xp * 0.5)
  }

  return { duskenCoin, bloodShards, xp }
}

export class BattleService {
  private pool: Pool | null = null
  private inMemoryBattles: Map<string, BattleRecord> = new Map()

  constructor(pool?: Pool) {
    this.pool = pool ?? null
  }

  async startBattle(request: StartBattleRequest): Promise<StartBattleResponse> {
    const { playerId, thrallId, wave } = request

    const seed = createSeed()
    const rng = mulberry32(seed)

    const thrallStats = DEFAULT_THRALL_STATS
    const enemies = generateEnemiesForWave(wave, BASE_ENEMY_STATS, rng)

    const outcome = resolveBattle(
      thrallStats,
      '[WEREWOLF]',
      enemies,
      seed
    )

    const rewards = calculateRewards(wave, outcome.result)

    const battleId = randomUUID()
    const battleRecord: BattleRecord = {
      id: battleId,
      playerId,
      thrallId,
      wave,
      seed,
      result: outcome.result,
      events: outcome.events,
      createdAt: new Date()
    }

    await this.storeBattle(battleRecord)

    return {
      battleId,
      result: outcome.result,
      events: outcome.events,
      rewards
    }
  }

  async getBattle(battleId: string): Promise<BattleRecord | null> {
    if (this.pool) {
      const result = await this.pool.query<{
        id: string
        player_id: string
        thrall_id: string
        wave: number
        seed: string
        result: BattleRecord['result']
        events: BattleRecord['events']
        created_at: Date
      }>(
        `SELECT id, player_id, thrall_id, wave, seed, result, events, created_at
         FROM battles WHERE id = $1`,
        [battleId]
      )

      if (result.rows.length === 0) return null

      const row = result.rows[0]
      return {
        id: row.id,
        playerId: row.player_id,
        thrallId: row.thrall_id,
        wave: row.wave,
        seed: parseInt(row.seed, 10),
        result: row.result,
        events: row.events,
        createdAt: row.created_at
      }
    }

    return this.inMemoryBattles.get(battleId) ?? null
  }

  async getBattlesByPlayer(
    playerId: string,
    limit = 20,
    offset = 0
  ): Promise<BattleRecord[]> {
    if (this.pool) {
      const result = await this.pool.query<{
        id: string
        player_id: string
        thrall_id: string
        wave: number
        seed: string
        result: BattleRecord['result']
        events: BattleRecord['events']
        created_at: Date
      }>(
        `SELECT id, player_id, thrall_id, wave, seed, result, events, created_at
         FROM battles
         WHERE player_id = $1
         ORDER BY created_at DESC
         LIMIT $2 OFFSET $3`,
        [playerId, limit, offset]
      )

      return result.rows.map((row) => ({
        id: row.id,
        playerId: row.player_id,
        thrallId: row.thrall_id,
        wave: row.wave,
        seed: parseInt(row.seed, 10),
        result: row.result,
        events: row.events,
        createdAt: row.created_at
      }))
    }

    const battles = Array.from(this.inMemoryBattles.values())
      .filter((b) => b.playerId === playerId)
      .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime())
      .slice(offset, offset + limit)

    return battles
  }

  async replayBattle(battleId: string): Promise<BattleOutcome | null> {
    const record = await this.getBattle(battleId)
    if (!record) return null

    const rng = mulberry32(record.seed)
    const enemies = generateEnemiesForWave(record.wave, BASE_ENEMY_STATS, rng)

    return resolveBattle(
      DEFAULT_THRALL_STATS,
      '[WEREWOLF]',
      enemies,
      record.seed
    )
  }

  private async storeBattle(battle: BattleRecord): Promise<void> {
    if (this.pool) {
      await this.pool.query(
        `INSERT INTO battles (id, player_id, thrall_id, wave, seed, result, events, created_at)
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8)`,
        [
          battle.id,
          battle.playerId,
          battle.thrallId,
          battle.wave,
          battle.seed.toString(),
          JSON.stringify(battle.result),
          JSON.stringify(battle.events),
          battle.createdAt
        ]
      )
      return
    }

    this.inMemoryBattles.set(battle.id, battle)
  }
}

export const battleService = new BattleService()

