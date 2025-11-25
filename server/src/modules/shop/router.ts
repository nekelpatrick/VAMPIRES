import { z } from 'zod'

import { procedure, router } from '../../trpc/core'
import { purchaseRequestSchema, purchaseResultSchema, shopItemSchema } from './shop.schema'
import { shopService } from './shop.service'

export const shopRouter = router({
  catalog: procedure
    .output(z.array(shopItemSchema))
    .query(() => {
      const items = shopService.getCatalog()
      return z.array(shopItemSchema).parse(items)
    }),

  item: procedure
    .input(z.object({ itemId: z.string().min(1) }))
    .output(shopItemSchema.nullable())
    .query(({ input }) => {
      const item = shopService.getItem(input.itemId)
      if (!item) return null
      return shopItemSchema.parse(item)
    }),

  purchase: procedure
    .input(purchaseRequestSchema)
    .output(purchaseResultSchema)
    .mutation(async ({ input }) => {
      const result = await shopService.purchase(input)
      return purchaseResultSchema.parse(result)
    }),

  xpMultiplier: procedure
    .input(z.object({ playerId: z.string().min(1) }))
    .output(z.object({ multiplier: z.number() }))
    .query(({ input }) => {
      return { multiplier: shopService.getXpMultiplier(input.playerId) }
    })
})

