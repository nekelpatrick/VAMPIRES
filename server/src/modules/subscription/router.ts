import { z } from 'zod'

import { procedure, router } from '../../trpc/core'
import {
  activateSubscriptionRequestSchema,
  subscriptionBenefitsSchema,
  subscriptionStatusSchema
} from './subscription.schema'
import { subscriptionService } from './subscription.service'

export const subscriptionRouter = router({
  status: procedure
    .input(z.object({ playerId: z.string().min(1) }))
    .output(subscriptionStatusSchema)
    .query(async ({ input }) => {
      const status = await subscriptionService.getSubscriptionStatus(input.playerId)
      return subscriptionStatusSchema.parse(status)
    }),

  benefits: procedure
    .input(z.object({ playerId: z.string().min(1) }))
    .output(subscriptionBenefitsSchema)
    .query(async ({ input }) => {
      const benefits = await subscriptionService.getBenefits(input.playerId)
      return subscriptionBenefitsSchema.parse(benefits)
    }),

  activate: procedure
    .input(activateSubscriptionRequestSchema)
    .output(subscriptionStatusSchema)
    .mutation(async ({ input }) => {
      const status = await subscriptionService.activateSubscription(input)
      return subscriptionStatusSchema.parse(status)
    }),

  cancel: procedure
    .input(z.object({ playerId: z.string().min(1) }))
    .output(subscriptionStatusSchema)
    .mutation(async ({ input }) => {
      const status = await subscriptionService.cancelSubscription(input.playerId)
      return subscriptionStatusSchema.parse(status)
    })
})

