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

  async addDuskenCoin(playerId: string, amount: number, _reason: string): Promise<CurrencyWallet> {
    return this.applyDelta({ playerId, duskenCoinDelta: amount, bloodShardDelta: 0 })
  }

  async addBloodShards(playerId: string, amount: number, _reason: string): Promise<CurrencyWallet> {
    return this.applyDelta({ playerId, duskenCoinDelta: 0, bloodShardDelta: amount })
  }

  async deductDuskenCoin(playerId: string, amount: number, _reason: string): Promise<CurrencyWallet | null> {
    const wallet = this.ensureWallet(playerId)
    if (wallet.duskenCoinBalance < amount) return null
    return this.applyDelta({ playerId, duskenCoinDelta: -amount, bloodShardDelta: 0 })
  }

  async deductBloodShards(playerId: string, amount: number, _reason: string): Promise<CurrencyWallet | null> {
    const wallet = this.ensureWallet(playerId)
    if (wallet.bloodShardBalance < amount) return null
    return this.applyDelta({ playerId, duskenCoinDelta: 0, bloodShardDelta: -amount })
  }
}

export const economyService = new EconomyService()

