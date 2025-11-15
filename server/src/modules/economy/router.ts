import { z } from 'zod'

import { procedure, router } from '../../trpc/core'
import { currencyWalletSchema, walletSyncInputSchema } from './economy.schema'
import { economyService } from './economy.service'

export const economyRouter = router({
  wallet: procedure
    .input(z.object({ playerId: z.string().min(1) }))
    .output(currencyWalletSchema)
    .query(async ({ input }) => {
      const wallet = await economyService.getWallet(input.playerId)
      return currencyWalletSchema.parse(wallet)
    }),
  syncWallet: procedure
    .input(walletSyncInputSchema)
    .output(currencyWalletSchema)
    .mutation(async ({ input }) => {
      const wallet = await economyService.applyDelta(input)
      return currencyWalletSchema.parse(wallet)
    })
})

