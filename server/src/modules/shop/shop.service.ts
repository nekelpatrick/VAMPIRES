import type { Pool } from 'pg'

import { economyService } from '../economy'
import { thrallService } from '../thrall'
import type { PurchaseRequest, PurchaseResult, ShopItem } from './shop.schema'
import { SHOP_CATALOG } from './shop.schema'

interface ActiveBooster {
  playerId: string
  type: string
  multiplier: number
  expiresAt: number
}

export class ShopService {
  private pool: Pool | null = null
  private activeBoosters: Map<string, ActiveBooster[]> = new Map()

  constructor(pool?: Pool) {
    this.pool = pool ?? null
  }

  getCatalog(): ShopItem[] {
    return SHOP_CATALOG.filter(item => item.available)
  }

  getItem(itemId: string): ShopItem | undefined {
    return SHOP_CATALOG.find(item => item.id === itemId)
  }

  async purchase(request: PurchaseRequest): Promise<PurchaseResult> {
    const item = this.getItem(request.itemId)
    if (!item) {
      return { success: false, error: 'Item not found' }
    }

    if (!item.available) {
      return { success: false, error: 'Item not available' }
    }

    const wallet = await economyService.getWallet(request.playerId)

    if (wallet.duskenCoinBalance < item.price.duskenCoin) {
      return { success: false, error: 'Insufficient [DUSKEN COIN]' }
    }

    if (wallet.bloodShardBalance < item.price.bloodShards) {
      return { success: false, error: 'Insufficient [BLOOD SHARDS]' }
    }

    if (item.price.duskenCoin > 0) {
      await economyService.deductDuskenCoin(request.playerId, item.price.duskenCoin, 'shop_purchase')
    }
    if (item.price.bloodShards > 0) {
      await economyService.deductBloodShards(request.playerId, item.price.bloodShards, 'shop_purchase')
    }

    await this.applyItemEffect(request.playerId, item)

    const updatedWallet = await economyService.getWallet(request.playerId)

    return {
      success: true,
      itemId: item.id,
      walletAfter: {
        duskenCoinBalance: updatedWallet.duskenCoinBalance,
        bloodShardBalance: updatedWallet.bloodShardBalance
      }
    }
  }

  private async applyItemEffect(playerId: string, item: ShopItem): Promise<void> {
    switch (item.type) {
      case 'DUSKEN_PACK':
        if (item.effect.value) {
          await economyService.addDuskenCoin(playerId, item.effect.value, 'shop_purchase')
        }
        break

      case 'XP_BOOSTER':
        if (item.effect.value && item.effect.duration) {
          this.addBooster(playerId, 'xp', item.effect.value, item.effect.duration)
        }
        break

      case 'STAT_BOOST':
        await this.applyStatBoost(playerId, item)
        break

      case 'REVIVE_TOKEN':
        break
    }
  }

  private async applyStatBoost(playerId: string, item: ShopItem): Promise<void> {
    if (!item.effect.stat || !item.effect.value) return

    const thrall = await thrallService.getThrallByOwner(playerId)
    if (!thrall) return

    const statColumn = item.effect.stat === 'maxHp' ? 'max_hp' : item.effect.stat

    if (this.pool) {
      await this.pool.query(
        `UPDATE thralls SET ${statColumn} = ${statColumn} + $1, updated_at = NOW() WHERE id = $2`,
        [item.effect.value, thrall.id]
      )
    }
  }

  private addBooster(playerId: string, type: string, multiplier: number, durationSeconds: number): void {
    const booster: ActiveBooster = {
      playerId,
      type,
      multiplier,
      expiresAt: Date.now() + durationSeconds * 1000
    }

    const playerBoosters = this.activeBoosters.get(playerId) ?? []
    playerBoosters.push(booster)
    this.activeBoosters.set(playerId, playerBoosters)
  }

  getActiveBooster(playerId: string, type: string): ActiveBooster | undefined {
    const playerBoosters = this.activeBoosters.get(playerId) ?? []
    const now = Date.now()

    const validBoosters = playerBoosters.filter(b => b.expiresAt > now)
    this.activeBoosters.set(playerId, validBoosters)

    return validBoosters.find(b => b.type === type)
  }

  getXpMultiplier(playerId: string): number {
    const booster = this.getActiveBooster(playerId, 'xp')
    return booster?.multiplier ?? 1
  }
}

export const shopService = new ShopService()

