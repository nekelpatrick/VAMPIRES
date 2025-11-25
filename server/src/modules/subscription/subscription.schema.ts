import { z } from 'zod'

export const subscriptionTierSchema = z.enum(['NONE', '[ASHEN ONE]'])

export type SubscriptionTier = z.infer<typeof subscriptionTierSchema>

export const subscriptionBenefitsSchema = z.object({
  offlineBonus: z.number().positive(),
  reviveDiscount: z.number().min(0).max(1),
  lootBonus: z.number().positive(),
  pvpPriority: z.boolean(),
  monthlyBloodShards: z.number().int().nonnegative()
})

export type SubscriptionBenefits = z.infer<typeof subscriptionBenefitsSchema>

export const subscriptionStatusSchema = z.object({
  playerId: z.string().min(1),
  tier: subscriptionTierSchema,
  active: z.boolean(),
  expiresAt: z.date().nullable(),
  benefits: subscriptionBenefitsSchema
})

export type SubscriptionStatus = z.infer<typeof subscriptionStatusSchema>

export const activateSubscriptionRequestSchema = z.object({
  playerId: z.string().min(1),
  durationDays: z.number().int().positive()
})

export type ActivateSubscriptionRequest = z.infer<typeof activateSubscriptionRequestSchema>

export const DEFAULT_BENEFITS: SubscriptionBenefits = {
  offlineBonus: 1.0,
  reviveDiscount: 0,
  lootBonus: 1.0,
  pvpPriority: false,
  monthlyBloodShards: 0
}

export const ASHEN_ONE_BENEFITS: SubscriptionBenefits = {
  offlineBonus: 1.5,
  reviveDiscount: 0.5,
  lootBonus: 1.25,
  pvpPriority: true,
  monthlyBloodShards: 50
}

