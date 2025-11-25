import { z } from 'zod'

export const thrallIdSchema = z.string().min(1)

export const thrallStatusSchema = z.enum(['ACTIVE', 'DEAD', 'REVIVING'])

export type ThrallStatus = z.infer<typeof thrallStatusSchema>

export const thrallSchema = z.object({
  id: thrallIdSchema,
  ownerId: z.string().min(1),
  archetype: z.enum(['[WEREWOLF]', '[THRALL]']),
  level: z.number().int().nonnegative().default(1),
  hp: z.number().nonnegative(),
  maxHp: z.number().nonnegative(),
  attack: z.number().nonnegative(),
  defense: z.number().nonnegative(),
  speed: z.number().positive().default(1),
  status: thrallStatusSchema.default('ACTIVE'),
  diedAt: z.date().nullable().optional(),
  reviveAt: z.date().nullable().optional(),
  pvpWins: z.number().int().nonnegative().default(0),
  pvpLosses: z.number().int().nonnegative().default(0),
  deathCount: z.number().int().nonnegative().default(0),
  createdAt: z.date().optional(),
  updatedAt: z.date().optional()
})

export type Thrall = z.infer<typeof thrallSchema>

export const createThrallRequestSchema = z.object({
  ownerId: z.string().min(1),
  archetype: z.enum(['[WEREWOLF]', '[THRALL]']).default('[WEREWOLF]')
})

export type CreateThrallRequest = z.infer<typeof createThrallRequestSchema>

export const getThrallRequestSchema = z.object({
  thrallId: thrallIdSchema
})

export type GetThrallRequest = z.infer<typeof getThrallRequestSchema>

export const updateThrallStatusRequestSchema = z.object({
  thrallId: thrallIdSchema,
  status: thrallStatusSchema,
  diedAt: z.date().nullable().optional(),
  reviveAt: z.date().nullable().optional()
})

export type UpdateThrallStatusRequest = z.infer<typeof updateThrallStatusRequestSchema>
