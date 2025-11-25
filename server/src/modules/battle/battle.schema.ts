import { z } from 'zod'

export const combatStatsSchema = z.object({
  maxHealth: z.number().positive(),
  attack: z.number().nonnegative(),
  defense: z.number().nonnegative(),
  speed: z.number().positive()
})

export type CombatStatsInput = z.infer<typeof combatStatsSchema>

export const battleEventTypeSchema = z.enum([
  'BATTLE_START',
  'TICK',
  'ATTACK',
  'DAMAGE',
  'DEATH',
  'BATTLE_END'
])

export const battleEventSchema = z.object({
  tick: z.number().int().nonnegative(),
  type: battleEventTypeSchema,
  actorId: z.string().optional(),
  targetId: z.string().optional(),
  value: z.number().optional(),
  data: z.record(z.unknown()).optional()
})

export type BattleEventInput = z.infer<typeof battleEventSchema>

export const battleResultSchema = z.object({
  winner: z.enum(['player', 'enemy', 'draw']),
  totalTicks: z.number().int().nonnegative(),
  thrallSurvived: z.boolean(),
  enemiesKilled: z.number().int().nonnegative(),
  damageDealt: z.number().int().nonnegative(),
  damageTaken: z.number().int().nonnegative()
})

export type BattleResultInput = z.infer<typeof battleResultSchema>

export const startBattleRequestSchema = z.object({
  playerId: z.string().min(1),
  thrallId: z.string().min(1),
  wave: z.number().int().positive()
})

export type StartBattleRequest = z.infer<typeof startBattleRequestSchema>

export const battleRecordSchema = z.object({
  id: z.string().uuid(),
  playerId: z.string().min(1),
  thrallId: z.string().min(1),
  wave: z.number().int().positive(),
  seed: z.number().int(),
  result: battleResultSchema,
  events: z.array(battleEventSchema),
  createdAt: z.date()
})

export type BattleRecord = z.infer<typeof battleRecordSchema>

export const getBattleRequestSchema = z.object({
  battleId: z.string().uuid()
})

export type GetBattleRequest = z.infer<typeof getBattleRequestSchema>

export const getBattlesByPlayerRequestSchema = z.object({
  playerId: z.string().min(1),
  limit: z.number().int().positive().max(100).default(20),
  offset: z.number().int().nonnegative().default(0)
})

export type GetBattlesByPlayerRequest = z.infer<typeof getBattlesByPlayerRequestSchema>

export const startBattleResponseSchema = z.object({
  battleId: z.string().uuid(),
  result: battleResultSchema,
  events: z.array(battleEventSchema),
  rewards: z.object({
    duskenCoin: z.number().int().nonnegative(),
    bloodShards: z.number().int().nonnegative(),
    xp: z.number().int().nonnegative()
  })
})

export type StartBattleResponse = z.infer<typeof startBattleResponseSchema>

