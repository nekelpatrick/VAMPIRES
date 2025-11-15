import { z } from 'zod'

export const currencyWalletSchema = z.object({
  playerId: z.string().min(1),
  duskenCoinBalance: z.number().nonnegative(),
  bloodShardBalance: z.number().nonnegative(),
  premiumStatus: z.enum(['NONE', '[PREMIUM]', '[ASHEN ONE]']).default('NONE')
})

export type CurrencyWallet = z.infer<typeof currencyWalletSchema>

export const walletSyncInputSchema = z.object({
  playerId: z.string().min(1),
  duskenCoinDelta: z.number(),
  bloodShardDelta: z.number()
})

export type WalletSyncInput = z.infer<typeof walletSyncInputSchema>

