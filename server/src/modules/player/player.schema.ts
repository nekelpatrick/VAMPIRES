import { z } from 'zod'

export const playerIdSchema = z.string().min(1)

export const clanSchema = z.enum(['CLAN NOCTURNUM', 'CLAN SABLEHEART', 'CLAN ECLIPSA'])

export const playerSchema = z.object({
  id: playerIdSchema,
  displayName: z.string().min(1),
  clan: clanSchema,
  duskenCoinBalance: z.number().nonnegative(),
  bloodShardBalance: z.number().nonnegative(),
  premiumStatus: z.enum(['NONE', '[ASHEN ONE]']).default('NONE')
})

export type Player = z.infer<typeof playerSchema>

