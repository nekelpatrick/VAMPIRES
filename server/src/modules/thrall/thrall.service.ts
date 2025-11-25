import type { Pool } from 'pg'

import type { Thrall, ThrallStatus, CreateThrallRequest } from './thrall.schema'

const BASE_STATS = {
  '[WEREWOLF]': { hp: 150, attack: 25, defense: 10, speed: 1.2 },
  '[THRALL]': { hp: 100, attack: 20, defense: 8, speed: 1.0 }
}

export class ThrallService {
  private pool: Pool | null = null
  private inMemoryThralls: Map<string, Thrall> = new Map()

  constructor(pool?: Pool) {
    this.pool = pool ?? null
  }

  async createThrall(request: CreateThrallRequest): Promise<Thrall> {
    const baseStats = BASE_STATS[request.archetype]
    const thrallId = `thrall-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`

    const thrall: Thrall = {
      id: thrallId,
      ownerId: request.ownerId,
      archetype: request.archetype,
      level: 1,
      hp: baseStats.hp,
      maxHp: baseStats.hp,
      attack: baseStats.attack,
      defense: baseStats.defense,
      speed: baseStats.speed,
      status: 'ACTIVE',
      diedAt: null,
      reviveAt: null,
      pvpWins: 0,
      pvpLosses: 0,
      deathCount: 0,
      createdAt: new Date(),
      updatedAt: new Date()
    }

    if (this.pool) {
      await this.pool.query(
        `INSERT INTO thralls (id, owner_id, archetype, level, hp, max_hp, attack, defense, speed, status, pvp_wins, pvp_losses, death_count, created_at, updated_at)
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15)`,
        [
          thrall.id, thrall.ownerId, thrall.archetype, thrall.level,
          thrall.hp, thrall.maxHp, thrall.attack, thrall.defense, thrall.speed,
          thrall.status, thrall.pvpWins, thrall.pvpLosses, thrall.deathCount,
          thrall.createdAt, thrall.updatedAt
        ]
      )
    } else {
      this.inMemoryThralls.set(thrall.id, thrall)
    }

    return thrall
  }

  async getThrall(thrallId: string): Promise<Thrall | null> {
    if (this.pool) {
      const result = await this.pool.query<{
        id: string
        owner_id: string
        archetype: '[WEREWOLF]' | '[THRALL]'
        level: number
        hp: number
        max_hp: number
        attack: number
        defense: number
        speed: number
        status: ThrallStatus
        died_at: Date | null
        revive_at: Date | null
        pvp_wins: number
        pvp_losses: number
        death_count: number
        created_at: Date
        updated_at: Date
      }>(
        `SELECT * FROM thralls WHERE id = $1`,
        [thrallId]
      )

      if (result.rows.length === 0) return null

      const row = result.rows[0]
      return {
        id: row.id,
        ownerId: row.owner_id,
        archetype: row.archetype,
        level: row.level,
        hp: row.hp,
        maxHp: row.max_hp,
        attack: row.attack,
        defense: row.defense,
        speed: row.speed,
        status: row.status,
        diedAt: row.died_at,
        reviveAt: row.revive_at,
        pvpWins: row.pvp_wins,
        pvpLosses: row.pvp_losses,
        deathCount: row.death_count,
        createdAt: row.created_at,
        updatedAt: row.updated_at
      }
    }

    return this.inMemoryThralls.get(thrallId) ?? null
  }

  async getThrallByOwner(ownerId: string): Promise<Thrall | null> {
    if (this.pool) {
      const result = await this.pool.query(
        `SELECT * FROM thralls WHERE owner_id = $1 LIMIT 1`,
        [ownerId]
      )

      if (result.rows.length === 0) return null
      return this.rowToThrall(result.rows[0])
    }

    for (const thrall of this.inMemoryThralls.values()) {
      if (thrall.ownerId === ownerId) return thrall
    }

    return null
  }

  async getOrCreateThrall(ownerId: string): Promise<Thrall> {
    let thrall = await this.getThrallByOwner(ownerId)

    if (!thrall) {
      thrall = await this.createThrall({ ownerId, archetype: '[WEREWOLF]' })
    }

    return thrall
  }

  async updateStatus(
    thrallId: string,
    status: ThrallStatus,
    diedAt?: Date | null,
    reviveAt?: Date | null
  ): Promise<Thrall | null> {
    const thrall = await this.getThrall(thrallId)
    if (!thrall) return null

    thrall.status = status
    thrall.updatedAt = new Date()

    if (diedAt !== undefined) thrall.diedAt = diedAt
    if (reviveAt !== undefined) thrall.reviveAt = reviveAt

    if (status === 'DEAD') {
      thrall.deathCount++
    }

    if (this.pool) {
      await this.pool.query(
        `UPDATE thralls SET status = $1, died_at = $2, revive_at = $3, death_count = $4, updated_at = $5 WHERE id = $6`,
        [status, thrall.diedAt, thrall.reviveAt, thrall.deathCount, thrall.updatedAt, thrallId]
      )
    } else {
      this.inMemoryThralls.set(thrallId, thrall)
    }

    return thrall
  }

  async recordPvpResult(thrallId: string, won: boolean): Promise<Thrall | null> {
    const thrall = await this.getThrall(thrallId)
    if (!thrall) return null

    if (won) {
      thrall.pvpWins++
    } else {
      thrall.pvpLosses++
    }
    thrall.updatedAt = new Date()

    if (this.pool) {
      await this.pool.query(
        `UPDATE thralls SET pvp_wins = $1, pvp_losses = $2, updated_at = $3 WHERE id = $4`,
        [thrall.pvpWins, thrall.pvpLosses, thrall.updatedAt, thrallId]
      )
    } else {
      this.inMemoryThralls.set(thrallId, thrall)
    }

    return thrall
  }

  async healThrall(thrallId: string): Promise<Thrall | null> {
    const thrall = await this.getThrall(thrallId)
    if (!thrall) return null

    thrall.hp = thrall.maxHp
    thrall.status = 'ACTIVE'
    thrall.diedAt = null
    thrall.reviveAt = null
    thrall.updatedAt = new Date()

    if (this.pool) {
      await this.pool.query(
        `UPDATE thralls SET hp = $1, status = $2, died_at = $3, revive_at = $4, updated_at = $5 WHERE id = $6`,
        [thrall.hp, thrall.status, thrall.diedAt, thrall.reviveAt, thrall.updatedAt, thrallId]
      )
    } else {
      this.inMemoryThralls.set(thrallId, thrall)
    }

    return thrall
  }

  calculatePowerScore(thrall: Thrall): number {
    return thrall.maxHp + thrall.attack * 2 + thrall.defense * 1.5
  }

  private rowToThrall(row: Record<string, unknown>): Thrall {
    return {
      id: row.id as string,
      ownerId: row.owner_id as string,
      archetype: row.archetype as '[WEREWOLF]' | '[THRALL]',
      level: row.level as number,
      hp: row.hp as number,
      maxHp: row.max_hp as number,
      attack: row.attack as number,
      defense: row.defense as number,
      speed: row.speed as number,
      status: row.status as ThrallStatus,
      diedAt: row.died_at as Date | null,
      reviveAt: row.revive_at as Date | null,
      pvpWins: row.pvp_wins as number,
      pvpLosses: row.pvp_losses as number,
      deathCount: row.death_count as number,
      createdAt: row.created_at as Date,
      updatedAt: row.updated_at as Date
    }
  }
}

export const thrallService = new ThrallService()
