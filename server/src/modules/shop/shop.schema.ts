import { z } from 'zod'

export const shopItemTypeSchema = z.enum([
  'REVIVE_TOKEN',
  'XP_BOOSTER',
  'DUSKEN_PACK',
  'BLOOD_SHARD_PACK',
  'STAT_BOOST'
])

export type ShopItemType = z.infer<typeof shopItemTypeSchema>

export const shopItemSchema = z.object({
  id: z.string().min(1),
  name: z.string().min(1),
  description: z.string(),
  type: shopItemTypeSchema,
  price: z.object({
    duskenCoin: z.number().int().nonnegative(),
    bloodShards: z.number().int().nonnegative()
  }),
  effect: z.object({
    stat: z.string().optional(),
    value: z.number().optional(),
    duration: z.number().optional()
  }),
  available: z.boolean().default(true)
})

export type ShopItem = z.infer<typeof shopItemSchema>

export const purchaseRequestSchema = z.object({
  playerId: z.string().min(1),
  itemId: z.string().min(1)
})

export type PurchaseRequest = z.infer<typeof purchaseRequestSchema>

export const purchaseResultSchema = z.object({
  success: z.boolean(),
  itemId: z.string().optional(),
  walletAfter: z.object({
    duskenCoinBalance: z.number().int().nonnegative(),
    bloodShardBalance: z.number().int().nonnegative()
  }).optional(),
  error: z.string().optional()
})

export type PurchaseResult = z.infer<typeof purchaseResultSchema>

export const SHOP_CATALOG: ShopItem[] = [
  {
    id: 'revive_token_1',
    name: 'Revive Token',
    description: 'Instantly revive your [THRALL] without waiting',
    type: 'REVIVE_TOKEN',
    price: { duskenCoin: 500, bloodShards: 0 },
    effect: { value: 1 },
    available: true
  },
  {
    id: 'revive_token_premium',
    name: 'Blood Revive',
    description: 'Premium instant revive with full HP restoration',
    type: 'REVIVE_TOKEN',
    price: { duskenCoin: 0, bloodShards: 5 },
    effect: { value: 1, stat: 'fullHp' },
    available: true
  },
  {
    id: 'xp_booster_30m',
    name: 'XP Booster (30 min)',
    description: '2x XP from battles for 30 minutes',
    type: 'XP_BOOSTER',
    price: { duskenCoin: 300, bloodShards: 0 },
    effect: { value: 2, duration: 1800 },
    available: true
  },
  {
    id: 'xp_booster_2h',
    name: 'XP Booster (2 hours)',
    description: '2x XP from battles for 2 hours',
    type: 'XP_BOOSTER',
    price: { duskenCoin: 0, bloodShards: 3 },
    effect: { value: 2, duration: 7200 },
    available: true
  },
  {
    id: 'dusken_pack_small',
    name: 'Small [DUSKEN COIN] Pack',
    description: 'Get 1000 [DUSKEN COIN]',
    type: 'DUSKEN_PACK',
    price: { duskenCoin: 0, bloodShards: 5 },
    effect: { value: 1000 },
    available: true
  },
  {
    id: 'dusken_pack_large',
    name: 'Large [DUSKEN COIN] Pack',
    description: 'Get 5000 [DUSKEN COIN]',
    type: 'DUSKEN_PACK',
    price: { duskenCoin: 0, bloodShards: 20 },
    effect: { value: 5000 },
    available: true
  },
  {
    id: 'stat_boost_attack',
    name: 'Attack Boost',
    description: 'Permanent +10 Attack for your [THRALL]',
    type: 'STAT_BOOST',
    price: { duskenCoin: 2000, bloodShards: 0 },
    effect: { stat: 'attack', value: 10 },
    available: true
  },
  {
    id: 'stat_boost_defense',
    name: 'Defense Boost',
    description: 'Permanent +8 Defense for your [THRALL]',
    type: 'STAT_BOOST',
    price: { duskenCoin: 1500, bloodShards: 0 },
    effect: { stat: 'defense', value: 8 },
    available: true
  },
  {
    id: 'stat_boost_hp',
    name: 'HP Boost',
    description: 'Permanent +50 Max HP for your [THRALL]',
    type: 'STAT_BOOST',
    price: { duskenCoin: 1800, bloodShards: 0 },
    effect: { stat: 'maxHp', value: 50 },
    available: true
  }
]

