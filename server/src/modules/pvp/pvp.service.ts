import { randomUUID } from 'crypto'

import type { Pool } from 'pg'

import { resolveBattle, createSeed, type CombatStats } from '../battle/combat-engine'
import { thrallService } from '../thrall/thrall.service'
import {
  calculatePowerScore,
  getThreatLevel,
  findMatch,
  isQueueExpired,
  generateBotOpponent,
  calculatePvpRewards,
  calculateLoserPenalty
} from './matchmaker'
import type {
  QueueEntry,
  PvpMatch,
  PvpBattleResult,
  QueueStatusResponse,
  MatchFoundResponse
} from './pvp.schema'

export class PvpService {
  private pool: Pool | null = null
  private queue: Map<string, QueueEntry> = new Map()
  private matches: Map<string, PvpMatch> = new Map()

  constructor(pool?: Pool) {
    this.pool = pool ?? null
  }

  async joinQueue(playerId: string, thrallId: string): Promise<{
    success: boolean
    error?: string
    matchFound?: MatchFoundResponse
  }> {
    if (this.queue.has(playerId)) {
      return { success: false, error: 'Already in queue' }
    }

    const thrall = await thrallService.getThrall(thrallId)
    if (!thrall) {
      return { success: false, error: 'Thrall not found' }
    }

    if (thrall.ownerId !== playerId) {
      return { success: false, error: 'Thrall does not belong to player' }
    }

    if (thrall.status !== 'ACTIVE') {
      return { success: false, error: 'Thrall is not active' }
    }

    const powerScore = calculatePowerScore(thrall)

    const entry: QueueEntry = {
      playerId,
      thrallId,
      powerScore,
      joinedAt: new Date(),
      status: 'WAITING'
    }

    this.queue.set(playerId, entry)

    const queueArray = Array.from(this.queue.values())
    const opponent = findMatch(entry, queueArray)

    if (opponent) {
      const match = await this.createMatch(entry, opponent)
      const opponentThrall = await thrallService.getThrall(opponent.thrallId)

      this.queue.delete(playerId)
      this.queue.delete(opponent.playerId)

      return {
        success: true,
        matchFound: {
          matchId: match.id,
          opponentThreatLevel: getThreatLevel(powerScore, opponent.powerScore),
          opponentArchetype: opponentThrall?.archetype ?? '[WEREWOLF]'
        }
      }
    }

    return { success: true }
  }

  async leaveQueue(playerId: string): Promise<boolean> {
    if (!this.queue.has(playerId)) {
      return false
    }

    this.queue.delete(playerId)
    return true
  }

  getQueueStatus(playerId: string): QueueStatusResponse {
    const entry = this.queue.get(playerId)

    if (!entry) {
      return { inQueue: false }
    }

    const position = Array.from(this.queue.values())
      .filter((e) => e.joinedAt <= entry.joinedAt)
      .length

    const waitTime = Math.floor((Date.now() - entry.joinedAt.getTime()) / 1000)

    return {
      inQueue: true,
      position,
      waitTime,
      estimatedWait: Math.max(0, 30 - waitTime)
    }
  }

  async processQueue(): Promise<void> {
    const entries = Array.from(this.queue.values())

    for (const entry of entries) {
      if (isQueueExpired(entry)) {
        const bot = generateBotOpponent(entry.powerScore)
        const botEntry: QueueEntry = {
          playerId: `bot-${Date.now()}`,
          thrallId: bot.thrallId,
          powerScore: bot.powerScore,
          joinedAt: new Date(),
          status: 'WAITING'
        }

        await this.createMatch(entry, botEntry, true)
        this.queue.delete(entry.playerId)
      }
    }
  }

  private async createMatch(
    player1: QueueEntry,
    player2: QueueEntry,
    isBot = false
  ): Promise<PvpMatch> {
    const match: PvpMatch = {
      id: randomUUID(),
      player1Id: player1.playerId,
      player1ThrallId: player1.thrallId,
      player2Id: player2.playerId,
      player2ThrallId: player2.thrallId,
      winnerId: null,
      loserId: null,
      battleId: null,
      status: 'PENDING',
      createdAt: new Date()
    }

    if (this.pool) {
      await this.pool.query(
        `INSERT INTO pvp_matches (id, player1_id, player1_thrall_id, player2_id, player2_thrall_id, status, created_at)
         VALUES ($1, $2, $3, $4, $5, $6, $7)`,
        [
          match.id, match.player1Id, match.player1ThrallId,
          match.player2Id, match.player2ThrallId, match.status, match.createdAt
        ]
      )
    }

    this.matches.set(match.id, match)

    if (isBot) {
      await this.resolvePvpBattle(match.id)
    }

    return match
  }

  async resolvePvpBattle(matchId: string): Promise<PvpBattleResult | null> {
    const match = this.matches.get(matchId)
    if (!match) return null

    if (match.status === 'COMPLETED') {
      return null
    }

    const isBot = match.player2Id.startsWith('bot-')

    const thrall1 = await thrallService.getThrall(match.player1ThrallId)
    if (!thrall1) return null

    let thrall2Stats: CombatStats
    let thrall2Level = 1

    if (isBot) {
      const botPowerScore = calculatePowerScore(thrall1) * (0.85 + Math.random() * 0.3)
      thrall2Stats = {
        maxHealth: Math.floor(botPowerScore * 0.4),
        attack: Math.floor(botPowerScore * 0.15),
        defense: Math.floor(botPowerScore * 0.1),
        speed: 1.0
      }
    } else {
      const thrall2 = await thrallService.getThrall(match.player2ThrallId)
      if (!thrall2) return null
      thrall2Stats = {
        maxHealth: thrall2.maxHp,
        attack: thrall2.attack,
        defense: thrall2.defense,
        speed: thrall2.speed
      }
      thrall2Level = thrall2.level
    }

    const seed = createSeed()
    const thrall1Stats: CombatStats = {
      maxHealth: thrall1.maxHp,
      attack: thrall1.attack,
      defense: thrall1.defense,
      speed: thrall1.speed
    }

    const outcome = resolveBattle(
      thrall1Stats,
      thrall1.archetype,
      [{ id: match.player2ThrallId, name: 'Opponent', stats: thrall2Stats }],
      seed
    )

    const player1Won = outcome.result.winner === 'player'
    const winnerId = player1Won ? match.player1Id : match.player2Id
    const loserId = player1Won ? match.player2Id : match.player1Id
    const winnerThrallId = player1Won ? match.player1ThrallId : match.player2ThrallId
    const loserThrallId = player1Won ? match.player2ThrallId : match.player1ThrallId
    const winnerLevel = player1Won ? thrall1.level : thrall2Level
    const loserLevel = player1Won ? thrall2Level : thrall1.level

    match.winnerId = winnerId
    match.loserId = loserId
    match.status = 'COMPLETED'
    match.battleId = randomUUID()

    this.matches.set(matchId, match)

    if (!isBot) {
      await thrallService.recordPvpResult(winnerThrallId, true)
      await thrallService.recordPvpResult(loserThrallId, false)
    } else {
      await thrallService.recordPvpResult(match.player1ThrallId, player1Won)
    }

    if (!player1Won) {
      await thrallService.updateStatus(match.player1ThrallId, 'DEAD', new Date(), null)
    } else if (!isBot) {
      await thrallService.updateStatus(match.player2ThrallId, 'DEAD', new Date(), null)
    }

    const rewards = calculatePvpRewards(winnerLevel, loserLevel, isBot)
    const penalty = calculateLoserPenalty(loserLevel)

    if (this.pool) {
      await this.pool.query(
        `UPDATE pvp_matches SET winner_id = $1, loser_id = $2, battle_id = $3, status = $4 WHERE id = $5`,
        [winnerId, loserId, match.battleId, match.status, matchId]
      )
    }

    return {
      matchId,
      battleId: match.battleId,
      winnerId,
      loserId,
      winnerRewards: rewards,
      loserPenalty: {
        thrallDied: true,
        rankingPoints: penalty.rankingPoints
      },
      events: outcome.events
    }
  }

  async getMatch(matchId: string): Promise<PvpMatch | null> {
    return this.matches.get(matchId) ?? null
  }

  async getPvpHistory(playerId: string, limit = 20, offset = 0): Promise<PvpMatch[]> {
    if (this.pool) {
      const result = await this.pool.query(
        `SELECT * FROM pvp_matches 
         WHERE player1_id = $1 OR player2_id = $1
         ORDER BY created_at DESC
         LIMIT $2 OFFSET $3`,
        [playerId, limit, offset]
      )

      return result.rows.map((row) => ({
        id: row.id,
        player1Id: row.player1_id,
        player1ThrallId: row.player1_thrall_id,
        player2Id: row.player2_id,
        player2ThrallId: row.player2_thrall_id,
        winnerId: row.winner_id,
        loserId: row.loser_id,
        battleId: row.battle_id,
        status: row.status,
        createdAt: row.created_at
      }))
    }

    return Array.from(this.matches.values())
      .filter((m) => m.player1Id === playerId || m.player2Id === playerId)
      .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime())
      .slice(offset, offset + limit)
  }
}

export const pvpService = new PvpService()

