import type { Pool } from 'pg'

import type { CurrencyWallet, WalletSyncInput } from './economy.schema'

export type TransactionReason =
  | 'quest_reward'
  | 'pvp_reward'
  | 'battle_reward'
  | 'revival_cost'
  | 'upgrade_cost'
  | 'shop_purchase'
  | 'subscription_bonus'
  | 'admin_adjustment'

export class EconomyService {
  private pool: Pool | null = null
  private readonly wallets = new Map<string, CurrencyWallet>()

  constructor(pool?: Pool) {
    this.pool = pool ?? null
  }

  private ensureWallet(playerId: string): CurrencyWallet {
    if (!this.wallets.has(playerId)) {
      this.wallets.set(playerId, {
        playerId,
        duskenCoinBalance: 1200,
        bloodShardBalance: 12,
        premiumStatus: 'NONE'
      })
    }
    return this.wallets.get(playerId)!
  }

  async getWallet(playerId: string): Promise<CurrencyWallet> {
    if (this.pool) {
      const result = await this.pool.query<{
        player_id: string
        dusken_coin_balance: number
        blood_shard_balance: number
        premium_status: string
        premium_expires_at: Date | null
      }>(
        `SELECT * FROM wallets WHERE player_id = $1`,
        [playerId]
      )

      if (result.rows.length === 0) {
        return this.createWallet(playerId)
      }

      const row = result.rows[0]
      return {
        playerId: row.player_id,
        duskenCoinBalance: row.dusken_coin_balance,
        bloodShardBalance: row.blood_shard_balance,
        premiumStatus: row.premium_status as CurrencyWallet['premiumStatus']
      }
    }

    return this.ensureWallet(playerId)
  }

  private async createWallet(playerId: string): Promise<CurrencyWallet> {
    const wallet: CurrencyWallet = {
      playerId,
      duskenCoinBalance: 1200,
      bloodShardBalance: 12,
      premiumStatus: 'NONE'
    }

    if (this.pool) {
      await this.pool.query(
        `INSERT INTO wallets (player_id, dusken_coin_balance, blood_shard_balance, premium_status)
         VALUES ($1, $2, $3, $4)
         ON CONFLICT (player_id) DO NOTHING`,
        [wallet.playerId, wallet.duskenCoinBalance, wallet.bloodShardBalance, wallet.premiumStatus]
      )
    } else {
      this.wallets.set(playerId, wallet)
    }

    return wallet
  }

  async applyDelta(input: WalletSyncInput): Promise<CurrencyWallet> {
    if (this.pool) {
      const wallet = await this.getWallet(input.playerId)
      const newDusken = Math.max(0, wallet.duskenCoinBalance + input.duskenCoinDelta)
      const newShards = Math.max(0, wallet.bloodShardBalance + input.bloodShardDelta)

      await this.pool.query(
        `UPDATE wallets 
         SET dusken_coin_balance = $1, blood_shard_balance = $2, updated_at = NOW()
         WHERE player_id = $3`,
        [newDusken, newShards, input.playerId]
      )

      return {
        ...wallet,
        duskenCoinBalance: newDusken,
        bloodShardBalance: newShards
      }
    }

    const wallet = this.ensureWallet(input.playerId)
    wallet.duskenCoinBalance = Math.max(0, wallet.duskenCoinBalance + input.duskenCoinDelta)
    wallet.bloodShardBalance = Math.max(0, wallet.bloodShardBalance + input.bloodShardDelta)
    return wallet
  }

  async addDuskenCoin(playerId: string, amount: number, reason: TransactionReason): Promise<CurrencyWallet> {
    const wallet = await this.applyDelta({ playerId, duskenCoinDelta: amount, bloodShardDelta: 0 })
    await this.logTransaction(playerId, '[DUSKEN COIN]', amount, reason)
    return wallet
  }

  async addBloodShards(playerId: string, amount: number, reason: TransactionReason): Promise<CurrencyWallet> {
    const wallet = await this.applyDelta({ playerId, duskenCoinDelta: 0, bloodShardDelta: amount })
    await this.logTransaction(playerId, '[BLOOD SHARDS]', amount, reason)
    return wallet
  }

  async deductDuskenCoin(playerId: string, amount: number, reason: TransactionReason): Promise<CurrencyWallet | null> {
    const wallet = await this.getWallet(playerId)
    if (wallet.duskenCoinBalance < amount) return null
    const updated = await this.applyDelta({ playerId, duskenCoinDelta: -amount, bloodShardDelta: 0 })
    await this.logTransaction(playerId, '[DUSKEN COIN]', -amount, reason)
    return updated
  }

  async deductBloodShards(playerId: string, amount: number, reason: TransactionReason): Promise<CurrencyWallet | null> {
    const wallet = await this.getWallet(playerId)
    if (wallet.bloodShardBalance < amount) return null
    const updated = await this.applyDelta({ playerId, duskenCoinDelta: 0, bloodShardDelta: -amount })
    await this.logTransaction(playerId, '[BLOOD SHARDS]', -amount, reason)
    return updated
  }

  async setPremiumStatus(
    playerId: string,
    status: CurrencyWallet['premiumStatus'],
    expiresAt?: Date
  ): Promise<CurrencyWallet> {
    const wallet = await this.getWallet(playerId)

    if (this.pool) {
      await this.pool.query(
        `UPDATE wallets 
         SET premium_status = $1, premium_expires_at = $2, updated_at = NOW()
         WHERE player_id = $3`,
        [status, expiresAt ?? null, playerId]
      )
    } else {
      wallet.premiumStatus = status
      this.wallets.set(playerId, wallet)
    }

    return { ...wallet, premiumStatus: status }
  }

  async isPremium(playerId: string): Promise<boolean> {
    const wallet = await this.getWallet(playerId)
    return wallet.premiumStatus === '[ASHEN ONE]' || wallet.premiumStatus === '[PREMIUM]'
  }

  private async logTransaction(
    playerId: string,
    currency: string,
    amount: number,
    reason: TransactionReason
  ): Promise<void> {
    if (!this.pool) return

    await this.pool.query(
      `INSERT INTO transactions (player_id, currency, amount, reason)
       VALUES ($1, $2, $3, $4)`,
      [playerId, currency, amount, reason]
    )
  }
}

export const economyService = new EconomyService()
