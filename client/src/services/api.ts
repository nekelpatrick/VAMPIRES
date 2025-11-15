import { z } from 'zod'

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

export const fetchHealth = async () => {
  try {
    const response = await fetch(`${API_BASE}/healthz`)
    if (!response.ok) {
      throw new Error('health check failed')
    }
    return response.json()
  } catch {
    return null
  }
}

export const fetchPlayerProfile = async (playerId = 'demo-player') => {
  const url = new URL('/trpc/player.getProfile', API_BASE)
  url.searchParams.set('input', JSON.stringify({ playerId }))

  try {
    const response = await fetch(url, { headers: { 'content-type': 'application/json' } })
    if (!response.ok) {
      throw new Error('profile request failed')
    }

    const payload = await response.json()
    const data = payload?.result?.data?.json ?? payload?.result?.data
    return playerProfileSchema.parse(data)
  } catch {
    return fallbackProfile
  }
}

