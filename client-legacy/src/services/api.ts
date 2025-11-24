import { z } from 'zod'

import { trpcClient } from './trpc'

const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:3000'

const playerProfileSchema = z.object({
  id: z.string(),
  displayName: z.string(),
  clan: z.string(),
  duskenCoinBalance: z.number(),
  bloodShardBalance: z.number(),
  premiumStatus: z.union([z.literal('NONE'), z.literal('[ASHEN ONE]')])
})

export type PlayerProfile = z.infer<typeof playerProfileSchema>

const fallbackProfile: PlayerProfile = {
  id: 'offline-demo',
  displayName: '[WEREWOLF]',
  clan: 'CLAN SABLEHEART',
  duskenCoinBalance: 420,
  bloodShardBalance: 3,
  premiumStatus: 'NONE'
}

const walletSchema = z.object({
  playerId: z.string(),
  duskenCoinBalance: z.number(),
  bloodShardBalance: z.number(),
  premiumStatus: z.union([z.literal('NONE'), z.literal('[PREMIUM]'), z.literal('[ASHEN ONE]')])
})

export type CurrencyWallet = z.infer<typeof walletSchema>

const walletSyncInputSchema = z.object({
  playerId: z.string().min(1),
  duskenCoinDelta: z.number(),
  bloodShardDelta: z.number()
})

export const fetchHealth = async () => {
  try {
    const response = await fetch(`${API_BASE}/healthz`)
    if (!response.ok) {
      throw new Error('health check failed')
    }
    return response.json()
  } catch (error) {
    console.warn('[api] Health check failed', error)
    return null
  }
}

export const fetchPlayerProfile = async (playerId = 'demo-player') => {
  try {
    const profile = await trpcClient.player.getProfile.query({ playerId })
    return playerProfileSchema.parse(profile)
  } catch (error) {
    console.error('[api] player.getProfile failed, using fallback', error)
    return fallbackProfile
  }
}

export const syncWallet = async (input: z.infer<typeof walletSyncInputSchema>) => {
  const payload = walletSyncInputSchema.parse(input)
  const wallet = await trpcClient.economy.syncWallet.mutate(payload)
  return walletSchema.parse(wallet)
}

