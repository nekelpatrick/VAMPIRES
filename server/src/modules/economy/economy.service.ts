import type { CurrencyWallet, WalletSyncInput } from './economy.schema'

export class EconomyService {
  private readonly wallets = new Map<string, CurrencyWallet>()

  private ensureWallet(playerId: string): CurrencyWallet {
    if (!this.wallets.has(playerId)) {
      this.wallets.set(playerId, {
      playerId,
      duskenCoinBalance: 1200,
      bloodShardBalance: 12,
      premiumStatus: '[PREMIUM]'
    })
    }
    return this.wallets.get(playerId)!
  }

  async getWallet(playerId: string): Promise<CurrencyWallet> {
    return this.ensureWallet(playerId)
  }

  async applyDelta(input: WalletSyncInput): Promise<CurrencyWallet> {
    const wallet = this.ensureWallet(input.playerId)
    wallet.duskenCoinBalance = Math.max(0, wallet.duskenCoinBalance + input.duskenCoinDelta)
    wallet.bloodShardBalance = Math.max(0, wallet.bloodShardBalance + input.bloodShardDelta)
    return wallet
  }
}

export const economyService = new EconomyService()

