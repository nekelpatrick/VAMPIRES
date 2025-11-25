import { z } from 'zod'

import { procedure, router } from '../../trpc/core'
import { thrallSchema } from './thrall.schema'
import { thrallService } from './thrall.service'

export const thrallRouter = router({
  active: procedure
    .input(z.object({ playerId: z.string().min(1) }))
    .output(thrallSchema)
    .query(async ({ input }) => {
      const thrall = await thrallService.getOrCreateThrall(input.playerId)
      return thrallSchema.parse(thrall)
    }),

  get: procedure
    .input(z.object({ thrallId: z.string().min(1) }))
    .query(async ({ input }) => {
      return thrallService.getThrall(input.thrallId)
    }),

  status: procedure
    .input(z.object({ thrallId: z.string().min(1) }))
    .query(async ({ input }) => {
      const thrall = await thrallService.getThrall(input.thrallId)
      if (!thrall) return null
      return {
        status: thrall.status,
        diedAt: thrall.diedAt,
        reviveAt: thrall.reviveAt
      }
    })
})
