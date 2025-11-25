import { z } from 'zod'

export const reviveMethodSchema = z.enum(['FREE', 'DUSKEN_COIN', 'BLOOD_SHARDS'])

export type ReviveMethod = z.infer<typeof reviveMethodSchema>

export const reviveRequestSchema = z.object({
  playerId: z.string().min(1),
  thrallId: z.string().min(1),
  method: reviveMethodSchema
})

export type ReviveRequest = z.infer<typeof reviveRequestSchema>

export const revivalStatusSchema = z.object({
  thrallId: z.string().min(1),
  status: z.enum(['DEAD', 'REVIVING', 'ACTIVE']),
  diedAt: z.date().nullable(),
  reviveAt: z.date().nullable(),
  timeRemaining: z.number().int().nonnegative().nullable(),
  canRevive: z.boolean(),
  revivalOptions: z.array(z.object({
    method: reviveMethodSchema,
    cost: z.number().int().nonnegative(),
    currency: z.enum(['[DUSKEN COIN]', '[BLOOD SHARDS]', 'NONE']),
    timeSeconds: z.number().int().nonnegative()
  }))
})

export type RevivalStatus = z.infer<typeof revivalStatusSchema>

export const reviveResponseSchema = z.object({
  success: z.boolean(),
  thrallId: z.string().min(1),
  method: reviveMethodSchema,
  cost: z.number().int().nonnegative(),
  reviveAt: z.date().nullable(),
  immediate: z.boolean()
})

export type ReviveResponse = z.infer<typeof reviveResponseSchema>

export const getRevivalStatusRequestSchema = z.object({
  thrallId: z.string().min(1)
})

export type GetRevivalStatusRequest = z.infer<typeof getRevivalStatusRequestSchema>

