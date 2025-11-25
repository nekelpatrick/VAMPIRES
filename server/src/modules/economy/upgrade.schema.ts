import { z } from 'zod'

export const upgradeStatSchema = z.enum(['attack', 'defense', 'maxHp', 'speed'])

export type UpgradeStat = z.infer<typeof upgradeStatSchema>

export const upgradeCostSchema = z.object({
  stat: upgradeStatSchema,
  currentLevel: z.number().int().nonnegative(),
  duskenCost: z.number().int().nonnegative(),
  bloodShardCost: z.number().int().nonnegative(),
  statIncrease: z.number().positive()
})

export type UpgradeCost = z.infer<typeof upgradeCostSchema>

export const upgradeRequestSchema = z.object({
  playerId: z.string().min(1),
  thrallId: z.string().min(1),
  stat: upgradeStatSchema
})

export type UpgradeRequest = z.infer<typeof upgradeRequestSchema>

export const upgradeResultSchema = z.object({
  success: z.boolean(),
  newStatValue: z.number().optional(),
  newLevel: z.number().int().optional(),
  walletAfter: z.object({
    duskenCoinBalance: z.number().int().nonnegative(),
    bloodShardBalance: z.number().int().nonnegative()
  }).optional(),
  error: z.string().optional()
})

export type UpgradeResult = z.infer<typeof upgradeResultSchema>

const BASE_COSTS: Record<UpgradeStat, { dusken: number; shards: number; increase: number }> = {
  attack: { dusken: 100, shards: 0, increase: 5 },
  defense: { dusken: 80, shards: 0, increase: 3 },
  maxHp: { dusken: 120, shards: 0, increase: 20 },
  speed: { dusken: 150, shards: 1, increase: 0.1 }
}

export function calculateUpgradeCost(stat: UpgradeStat, currentLevel: number): UpgradeCost {
  const base = BASE_COSTS[stat]
  const multiplier = 1 + currentLevel * 0.25

  return {
    stat,
    currentLevel,
    duskenCost: Math.floor(base.dusken * multiplier),
    bloodShardCost: Math.floor(base.shards * multiplier),
    statIncrease: base.increase
  }
}

export function getStatIncreaseForLevel(stat: UpgradeStat, _level: number): number {
  return BASE_COSTS[stat].increase
}

