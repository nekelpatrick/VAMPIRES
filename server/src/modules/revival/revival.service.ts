import { thrallService } from '../thrall/thrall.service'
import type { Thrall } from '../thrall/thrall.schema'
import type {
  ReviveMethod,
  ReviveRequest,
  ReviveResponse,
  RevivalStatus
} from './revival.schema'

const FREE_REVIVE_SECONDS = 4 * 60 * 60
const DUSKEN_REVIVE_SECONDS = 1 * 60 * 60
const BLOOD_SHARDS_REVIVE_SECONDS = 0
const ASHEN_ONE_DISCOUNT = 0.5

interface RevivalCost {
  duskenCoin: number
  bloodShards: number
  timeSeconds: number
}

function calculateRevivalCosts(thrall: Thrall, isPremium: boolean): Record<ReviveMethod, RevivalCost> {
  const level = thrall.level
  const discount = isPremium ? ASHEN_ONE_DISCOUNT : 1

  return {
    FREE: {
      duskenCoin: 0,
      bloodShards: 0,
      timeSeconds: Math.floor(FREE_REVIVE_SECONDS * (isPremium ? 0.5 : 1))
    },
    DUSKEN_COIN: {
      duskenCoin: Math.floor((100 + level * 10) * discount),
      bloodShards: 0,
      timeSeconds: Math.floor(DUSKEN_REVIVE_SECONDS * (isPremium ? 0.5 : 1))
    },
    BLOOD_SHARDS: {
      duskenCoin: 0,
      bloodShards: Math.floor((5 + level) * discount),
      timeSeconds: BLOOD_SHARDS_REVIVE_SECONDS
    }
  }
}

export class RevivalService {
  async getRevivalStatus(thrallId: string, isPremium = false): Promise<RevivalStatus | null> {
    const thrall = await thrallService.getThrall(thrallId)
    if (!thrall) return null

    const costs = calculateRevivalCosts(thrall, isPremium)

    let timeRemaining: number | null = null
    if (thrall.status === 'REVIVING' && thrall.reviveAt) {
      timeRemaining = Math.max(0, Math.floor((thrall.reviveAt.getTime() - Date.now()) / 1000))

      if (timeRemaining === 0) {
        await this.completeRevival(thrallId)
        return this.getRevivalStatus(thrallId, isPremium)
      }
    }

    const canRevive = thrall.status === 'DEAD'

    return {
      thrallId,
      status: thrall.status,
      diedAt: thrall.diedAt ?? null,
      reviveAt: thrall.reviveAt ?? null,
      timeRemaining,
      canRevive,
      revivalOptions: [
        {
          method: 'FREE',
          cost: costs.FREE.duskenCoin,
          currency: 'NONE',
          timeSeconds: costs.FREE.timeSeconds
        },
        {
          method: 'DUSKEN_COIN',
          cost: costs.DUSKEN_COIN.duskenCoin,
          currency: '[DUSKEN COIN]',
          timeSeconds: costs.DUSKEN_COIN.timeSeconds
        },
        {
          method: 'BLOOD_SHARDS',
          cost: costs.BLOOD_SHARDS.bloodShards,
          currency: '[BLOOD SHARDS]',
          timeSeconds: costs.BLOOD_SHARDS.timeSeconds
        }
      ]
    }
  }

  async startRevival(
    request: ReviveRequest,
    isPremium = false,
    playerBalance?: { duskenCoin: number; bloodShards: number }
  ): Promise<ReviveResponse & { error?: string }> {
    const thrall = await thrallService.getThrall(request.thrallId)

    if (!thrall) {
      return {
        success: false,
        thrallId: request.thrallId,
        method: request.method,
        cost: 0,
        reviveAt: null,
        immediate: false,
        error: 'Thrall not found'
      }
    }

    if (thrall.ownerId !== request.playerId) {
      return {
        success: false,
        thrallId: request.thrallId,
        method: request.method,
        cost: 0,
        reviveAt: null,
        immediate: false,
        error: 'Thrall does not belong to player'
      }
    }

    if (thrall.status !== 'DEAD') {
      return {
        success: false,
        thrallId: request.thrallId,
        method: request.method,
        cost: 0,
        reviveAt: null,
        immediate: false,
        error: 'Thrall is not dead'
      }
    }

    const costs = calculateRevivalCosts(thrall, isPremium)
    const methodCost = costs[request.method]

    if (playerBalance) {
      if (request.method === 'DUSKEN_COIN' && playerBalance.duskenCoin < methodCost.duskenCoin) {
        return {
          success: false,
          thrallId: request.thrallId,
          method: request.method,
          cost: methodCost.duskenCoin,
          reviveAt: null,
          immediate: false,
          error: 'Insufficient [DUSKEN COIN]'
        }
      }

      if (request.method === 'BLOOD_SHARDS' && playerBalance.bloodShards < methodCost.bloodShards) {
        return {
          success: false,
          thrallId: request.thrallId,
          method: request.method,
          cost: methodCost.bloodShards,
          reviveAt: null,
          immediate: false,
          error: 'Insufficient [BLOOD SHARDS]'
        }
      }
    }

    const immediate = methodCost.timeSeconds === 0
    const reviveAt = immediate
      ? null
      : new Date(Date.now() + methodCost.timeSeconds * 1000)

    if (immediate) {
      await thrallService.healThrall(request.thrallId)
    } else {
      await thrallService.updateStatus(
        request.thrallId,
        'REVIVING',
        thrall.diedAt,
        reviveAt
      )
    }

    const cost = request.method === 'DUSKEN_COIN'
      ? methodCost.duskenCoin
      : request.method === 'BLOOD_SHARDS'
        ? methodCost.bloodShards
        : 0

    return {
      success: true,
      thrallId: request.thrallId,
      method: request.method,
      cost,
      reviveAt,
      immediate
    }
  }

  async completeRevival(thrallId: string): Promise<boolean> {
    const thrall = await thrallService.getThrall(thrallId)

    if (!thrall || thrall.status !== 'REVIVING') {
      return false
    }

    if (thrall.reviveAt && thrall.reviveAt.getTime() > Date.now()) {
      return false
    }

    await thrallService.healThrall(thrallId)
    return true
  }

  async checkAndCompleteRevival(thrallId: string): Promise<boolean> {
    const thrall = await thrallService.getThrall(thrallId)

    if (!thrall || thrall.status !== 'REVIVING') {
      return false
    }

    if (!thrall.reviveAt || thrall.reviveAt.getTime() > Date.now()) {
      return false
    }

    return this.completeRevival(thrallId)
  }
}

export const revivalService = new RevivalService()

