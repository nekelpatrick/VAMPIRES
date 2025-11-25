import type { Thrall } from '../thrall/thrall.schema'
import type { QueueEntry, ThreatLevel } from './pvp.schema'

const MATCH_RANGE_PERCENT = 0.2
const QUEUE_TIMEOUT_MS = 30000

export function calculatePowerScore(thrall: Thrall): number {
  return thrall.maxHp + thrall.attack * 2 + thrall.defense * 1.5
}

export function getThreatLevel(myPowerScore: number, opponentPowerScore: number): ThreatLevel {
  const ratio = opponentPowerScore / myPowerScore

  if (ratio < 0.8) return 'LOW'
  if (ratio <= 1.2) return 'MODERATE'
  return 'HIGH'
}

export function isWithinMatchRange(score1: number, score2: number): boolean {
  const lower = Math.min(score1, score2)
  const higher = Math.max(score1, score2)
  const range = lower * MATCH_RANGE_PERCENT

  return higher <= lower + range * 2
}

export function findMatch(
  entry: QueueEntry,
  queue: QueueEntry[]
): QueueEntry | null {
  const candidates = queue.filter((other) => {
    if (other.playerId === entry.playerId) return false
    if (other.status !== 'WAITING') return false
    return isWithinMatchRange(entry.powerScore, other.powerScore)
  })

  if (candidates.length === 0) return null

  candidates.sort((a, b) => {
    const diffA = Math.abs(a.powerScore - entry.powerScore)
    const diffB = Math.abs(b.powerScore - entry.powerScore)
    return diffA - diffB
  })

  return candidates[0]
}

export function isQueueExpired(entry: QueueEntry): boolean {
  const elapsed = Date.now() - entry.joinedAt.getTime()
  return elapsed >= QUEUE_TIMEOUT_MS
}

export function generateBotOpponent(playerPowerScore: number): {
  thrallId: string
  powerScore: number
  archetype: '[WEREWOLF]' | '[THRALL]'
} {
  const variance = (Math.random() - 0.5) * 0.3
  const botPowerScore = playerPowerScore * (1 + variance)

  return {
    thrallId: `bot-${Date.now()}`,
    powerScore: botPowerScore,
    archetype: '[WEREWOLF]'
  }
}

export function calculatePvpRewards(
  winnerLevel: number,
  loserLevel: number,
  wasBot: boolean
): {
  duskenCoin: number
  bloodShards: number
  rankingPoints: number
} {
  const baseReward = 50 + winnerLevel * 10
  const levelDiff = loserLevel - winnerLevel
  const diffBonus = Math.max(0, levelDiff * 5)

  let duskenCoin = baseReward + diffBonus
  let bloodShards = 0
  let rankingPoints = 10 + Math.max(0, levelDiff)

  if (wasBot) {
    duskenCoin = Math.floor(duskenCoin * 0.5)
    rankingPoints = Math.floor(rankingPoints * 0.5)
  }

  if (Math.random() < 0.1) {
    bloodShards = 1
  }

  return { duskenCoin, bloodShards, rankingPoints }
}

export function calculateLoserPenalty(loserLevel: number): {
  rankingPoints: number
} {
  return {
    rankingPoints: -Math.max(5, loserLevel)
  }
}

