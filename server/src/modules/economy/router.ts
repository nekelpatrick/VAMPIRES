import { z } from 'zod'

import { procedure, router } from '../../trpc/core'
import { currencyWalletSchema, walletSyncInputSchema } from './economy.schema'
import { economyService } from './economy.service'
import { upgradeCostSchema, upgradeRequestSchema, upgradeResultSchema, upgradeStatSchema } from './upgrade.schema'
import { upgradeService } from './upgrade.service'

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
    }),

  upgradeCost: procedure
    .input(z.object({
      thrallId: z.string().min(1),
      stat: upgradeStatSchema
    }))
    .output(upgradeCostSchema)
    .query(({ input }) => {
      const cost = upgradeService.getUpgradeCost(input.thrallId, input.stat)
      return upgradeCostSchema.parse(cost)
    }),

  allUpgradeCosts: procedure
    .input(z.object({ thrallId: z.string().min(1) }))
    .output(z.array(upgradeCostSchema))
    .query(({ input }) => {
      const costs = upgradeService.getAllUpgradeCosts(input.thrallId)
      return z.array(upgradeCostSchema).parse(costs)
    }),

  upgradeThrall: procedure
    .input(upgradeRequestSchema)
    .output(upgradeResultSchema)
    .mutation(async ({ input }) => {
      const result = await upgradeService.upgradeThrall(input)
      return upgradeResultSchema.parse(result)
    })
})
