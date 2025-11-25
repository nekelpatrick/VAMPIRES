import { z } from 'zod'

export const threatLevelSchema = z.enum(['LOW', 'MODERATE', 'HIGH', 'UNKNOWN'])

export type ThreatLevel = z.infer<typeof threatLevelSchema>

export const queueStatusSchema = z.enum(['WAITING', 'MATCHED', 'CANCELLED', 'EXPIRED'])

export type QueueStatus = z.infer<typeof queueStatusSchema>

export const joinQueueRequestSchema = z.object({
  playerId: z.string().min(1),
  thrallId: z.string().min(1)
})

export type JoinQueueRequest = z.infer<typeof joinQueueRequestSchema>

export const leaveQueueRequestSchema = z.object({
  playerId: z.string().min(1)
})

export type LeaveQueueRequest = z.infer<typeof leaveQueueRequestSchema>

export const queueEntrySchema = z.object({
  playerId: z.string().min(1),
  thrallId: z.string().min(1),
  powerScore: z.number().positive(),
  joinedAt: z.date(),
  status: queueStatusSchema
})

export type QueueEntry = z.infer<typeof queueEntrySchema>

export const queueStatusResponseSchema = z.object({
  inQueue: z.boolean(),
  position: z.number().int().nonnegative().optional(),
  waitTime: z.number().int().nonnegative().optional(),
  estimatedWait: z.number().int().nonnegative().optional()
})

export type QueueStatusResponse = z.infer<typeof queueStatusResponseSchema>

export const matchFoundResponseSchema = z.object({
  matchId: z.string().uuid(),
  opponentThreatLevel: threatLevelSchema,
  opponentArchetype: z.string()
})

export type MatchFoundResponse = z.infer<typeof matchFoundResponseSchema>

export const pvpMatchSchema = z.object({
  id: z.string().uuid(),
  player1Id: z.string().min(1),
  player1ThrallId: z.string().min(1),
  player2Id: z.string().min(1),
  player2ThrallId: z.string().min(1),
  winnerId: z.string().nullable(),
  loserId: z.string().nullable(),
  battleId: z.string().uuid().nullable(),
  status: z.enum(['PENDING', 'IN_PROGRESS', 'COMPLETED', 'CANCELLED']),
  createdAt: z.date()
})

export type PvpMatch = z.infer<typeof pvpMatchSchema>

export const pvpBattleRequestSchema = z.object({
  matchId: z.string().uuid()
})

export type PvpBattleRequest = z.infer<typeof pvpBattleRequestSchema>

export const pvpBattleResultSchema = z.object({
  matchId: z.string().uuid(),
  battleId: z.string().uuid(),
  winnerId: z.string(),
  loserId: z.string(),
  winnerRewards: z.object({
    duskenCoin: z.number().int().nonnegative(),
    bloodShards: z.number().int().nonnegative(),
    rankingPoints: z.number().int()
  }),
  loserPenalty: z.object({
    thrallDied: z.boolean(),
    rankingPoints: z.number().int()
  }),
  events: z.array(z.unknown())
})

export type PvpBattleResult = z.infer<typeof pvpBattleResultSchema>

export const getPvpHistoryRequestSchema = z.object({
  playerId: z.string().min(1),
  limit: z.number().int().positive().max(50).default(20),
  offset: z.number().int().nonnegative().default(0)
})

export type GetPvpHistoryRequest = z.infer<typeof getPvpHistoryRequestSchema>

