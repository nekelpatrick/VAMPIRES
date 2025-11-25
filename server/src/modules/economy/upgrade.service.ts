import type { Pool } from 'pg'

import { thrallService } from '../thrall'
import { economyService } from './economy.service'
import type { UpgradeCost, UpgradeRequest, UpgradeResult, UpgradeStat } from './upgrade.schema'
import { calculateUpgradeCost, getStatIncreaseForLevel } from './upgrade.schema'

const STAT_LEVEL_MAP: Record<string, Record<UpgradeStat, number>> = {}

export class UpgradeService {
  private pool: Pool | null = null

  constructor(pool?: Pool) {
    this.pool = pool ?? null
  }

  getStatLevel(thrallId: string, stat: UpgradeStat): number {
    return STAT_LEVEL_MAP[thrallId]?.[stat] ?? 0
  }

  private setStatLevel(thrallId: string, stat: UpgradeStat, level: number): void {
    if (!STAT_LEVEL_MAP[thrallId]) {
      STAT_LEVEL_MAP[thrallId] = { attack: 0, defense: 0, maxHp: 0, speed: 0 }
    }
    STAT_LEVEL_MAP[thrallId][stat] = level
  }

  getUpgradeCost(thrallId: string, stat: UpgradeStat): UpgradeCost {
    const currentLevel = this.getStatLevel(thrallId, stat)
    return calculateUpgradeCost(stat, currentLevel)
  }

  getAllUpgradeCosts(thrallId: string): UpgradeCost[] {
    const stats: UpgradeStat[] = ['attack', 'defense', 'maxHp', 'speed']
    return stats.map(stat => this.getUpgradeCost(thrallId, stat))
  }

  async upgradeThrall(request: UpgradeRequest): Promise<UpgradeResult> {
    const thrall = await thrallService.getThrall(request.thrallId)
    if (!thrall) {
      return { success: false, error: 'Thrall not found' }
    }

    if (thrall.ownerId !== request.playerId) {
      return { success: false, error: 'Thrall does not belong to player' }
    }

    if (thrall.status === 'DEAD') {
      return { success: false, error: 'Cannot upgrade dead thrall' }
    }

    const cost = this.getUpgradeCost(request.thrallId, request.stat)
    const wallet = await economyService.getWallet(request.playerId)

    if (wallet.duskenCoinBalance < cost.duskenCost) {
      return { success: false, error: 'Insufficient [DUSKEN COIN]' }
    }

    if (wallet.bloodShardBalance < cost.bloodShardCost) {
      return { success: false, error: 'Insufficient [BLOOD SHARDS]' }
    }

    if (cost.duskenCost > 0) {
      await economyService.deductDuskenCoin(request.playerId, cost.duskenCost, 'upgrade_cost')
    }
    if (cost.bloodShardCost > 0) {
      await economyService.deductBloodShards(request.playerId, cost.bloodShardCost, 'upgrade_cost')
    }

    const newLevel = cost.currentLevel + 1
    this.setStatLevel(request.thrallId, request.stat, newLevel)

    const statIncrease = getStatIncreaseForLevel(request.stat, newLevel)
    const newStatValue = await this.applyStatIncrease(request.thrallId, request.stat, statIncrease)

    const updatedWallet = await economyService.getWallet(request.playerId)

    return {
      success: true,
      newStatValue,
      newLevel,
      walletAfter: {
        duskenCoinBalance: updatedWallet.duskenCoinBalance,
        bloodShardBalance: updatedWallet.bloodShardBalance
      }
    }
  }

  private async applyStatIncrease(thrallId: string, stat: UpgradeStat, increase: number): Promise<number> {
    const thrall = await thrallService.getThrall(thrallId)
    if (!thrall) return 0

    let newValue = 0

    switch (stat) {
      case 'attack':
        newValue = thrall.attack + increase
        break
      case 'defense':
        newValue = thrall.defense + increase
        break
      case 'maxHp':
        newValue = thrall.maxHp + increase
        break
      case 'speed':
        newValue = thrall.speed + increase
        break
    }

    if (this.pool) {
      await this.pool.query(
        `UPDATE thralls SET ${stat === 'maxHp' ? 'max_hp' : stat} = $1, updated_at = NOW() WHERE id = $2`,
        [newValue, thrallId]
      )
    }

    return newValue
  }
}

export const upgradeService = new UpgradeService()

