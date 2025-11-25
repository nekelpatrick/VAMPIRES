import { z } from 'zod'

export const questTypeSchema = z.enum([
  'KillEnemies',
  'ReachWave',
  'DealDamage',
  'WinPvP',
  'GetCombo',
  'EarnCurrency',
  'SurviveWaves'
])

export type QuestType = z.infer<typeof questTypeSchema>

export const questProgressSchema = z.object({
  id: z.string().uuid(),
  playerId: z.string().min(1),
  questType: questTypeSchema,
  questName: z.string().min(1),
  description: z.string().nullable(),
  targetValue: z.number().int().positive(),
  currentValue: z.number().int().nonnegative(),
  duskenReward: z.number().int().nonnegative(),
  bloodShardsReward: z.number().int().nonnegative(),
  completed: z.boolean(),
  claimed: z.boolean(),
  adBonusClaimed: z.boolean(),
  questDate: z.string()
})

export type QuestProgress = z.infer<typeof questProgressSchema>

export const getDailyQuestsRequestSchema = z.object({
  playerId: z.string().min(1)
})

export type GetDailyQuestsRequest = z.infer<typeof getDailyQuestsRequestSchema>

export const updateQuestProgressRequestSchema = z.object({
  playerId: z.string().min(1),
  questType: questTypeSchema,
  value: z.number().int().nonnegative()
})

export type UpdateQuestProgressRequest = z.infer<typeof updateQuestProgressRequestSchema>

export const claimQuestRequestSchema = z.object({
  playerId: z.string().min(1),
  questId: z.string().uuid(),
  watchedAd: z.boolean().default(false)
})

export type ClaimQuestRequest = z.infer<typeof claimQuestRequestSchema>

export const claimQuestResponseSchema = z.object({
  success: z.boolean(),
  questId: z.string().uuid(),
  duskenAwarded: z.number().int().nonnegative(),
  bloodShardsAwarded: z.number().int().nonnegative(),
  adBonusApplied: z.boolean()
})

export type ClaimQuestResponse = z.infer<typeof claimQuestResponseSchema>

export const questDefinitionSchema = z.object({
  questType: questTypeSchema,
  questName: z.string(),
  description: z.string(),
  targetValue: z.number().int().positive(),
  duskenReward: z.number().int().nonnegative(),
  bloodShardsReward: z.number().int().nonnegative()
})

export type QuestDefinition = z.infer<typeof questDefinitionSchema>

