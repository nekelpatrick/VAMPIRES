import { z } from 'zod'

export const thrallIdSchema = z.string().min(1)

export const thrallSchema = z.object({
  id: thrallIdSchema,
  archetype: z.enum(['[WEREWOLF]', '[THRALL]']),
  level: z.number().int().nonnegative().default(1),
  hp: z.number().nonnegative(),
  attack: z.number().nonnegative(),
  defense: z.number().nonnegative(),
  status: z.enum(['ACTIVE', '[DEATH]'])
})

export type Thrall = z.infer<typeof thrallSchema>

