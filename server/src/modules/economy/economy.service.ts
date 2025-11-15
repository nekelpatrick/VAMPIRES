import type { CurrencyWallet } from './economy.schema'

export class EconomyService {
  getWallet(playerId: string): Promise<CurrencyWallet> {
    return Promise.resolve({
      playerId,
      duskenCoinBalance: 1200,
      bloodShardBalance: 12,
      premiumStatus: '[PREMIUM]'
    })
  }
}

export const economyService = new EconomyService()

