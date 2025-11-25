import type { Pool } from 'pg'

import { economyService } from '../economy'
import type { ActivateSubscriptionRequest, SubscriptionBenefits, SubscriptionStatus } from './subscription.schema'
import { ASHEN_ONE_BENEFITS, DEFAULT_BENEFITS } from './subscription.schema'

export class SubscriptionService {
  private pool: Pool | null = null
  private inMemorySubscriptions: Map<string, { expiresAt: Date | null }> = new Map()

  constructor(pool?: Pool) {
    this.pool = pool ?? null
  }

  async getSubscriptionStatus(playerId: string): Promise<SubscriptionStatus> {
    const wallet = await economyService.getWallet(playerId)
    const isPremium = wallet.premiumStatus === '[ASHEN ONE]'

    let expiresAt: Date | null = null

    if (this.pool) {
      const result = await this.pool.query<{ premium_expires_at: Date | null }>(
        `SELECT premium_expires_at FROM wallets WHERE player_id = $1`,
        [playerId]
      )
      if (result.rows.length > 0) {
        expiresAt = result.rows[0].premium_expires_at
      }
    } else {
      expiresAt = this.inMemorySubscriptions.get(playerId)?.expiresAt ?? null
    }

    const active = isPremium && (!expiresAt || expiresAt > new Date())

    return {
      playerId,
      tier: active ? '[ASHEN ONE]' : 'NONE',
      active,
      expiresAt,
      benefits: active ? ASHEN_ONE_BENEFITS : DEFAULT_BENEFITS
    }
  }

  async getBenefits(playerId: string): Promise<SubscriptionBenefits> {
    const status = await this.getSubscriptionStatus(playerId)
    return status.benefits
  }

  async isSubscribed(playerId: string): Promise<boolean> {
    const status = await this.getSubscriptionStatus(playerId)
    return status.active
  }

  async activateSubscription(request: ActivateSubscriptionRequest): Promise<SubscriptionStatus> {
    const expiresAt = new Date()
    expiresAt.setDate(expiresAt.getDate() + request.durationDays)

    await economyService.setPremiumStatus(request.playerId, '[ASHEN ONE]', expiresAt)

    if (!this.pool) {
      this.inMemorySubscriptions.set(request.playerId, { expiresAt })
    }

    await economyService.addBloodShards(
      request.playerId,
      ASHEN_ONE_BENEFITS.monthlyBloodShards,
      'subscription_bonus'
    )

    return this.getSubscriptionStatus(request.playerId)
  }

  async cancelSubscription(playerId: string): Promise<SubscriptionStatus> {
    await economyService.setPremiumStatus(playerId, 'NONE')

    if (!this.pool) {
      this.inMemorySubscriptions.delete(playerId)
    }

    return this.getSubscriptionStatus(playerId)
  }

  getReviveDiscount(benefits: SubscriptionBenefits): number {
    return benefits.reviveDiscount
  }

  getLootMultiplier(benefits: SubscriptionBenefits): number {
    return benefits.lootBonus
  }

  getOfflineMultiplier(benefits: SubscriptionBenefits): number {
    return benefits.offlineBonus
  }
}

export const subscriptionService = new SubscriptionService()

