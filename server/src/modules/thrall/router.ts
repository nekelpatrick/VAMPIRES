import { z } from 'zod'

import { procedure, router } from '../../trpc/core'
import { thrallSchema } from './thrall.schema'
import { thrallService } from './thrall.service'

export const thrallRouter = router({
  active: procedure
    .input(z.object({ playerId: z.string().min(1) }))
    .output(thrallSchema)
    .query(async ({ input }) => {
      const thrall = await thrallService.getActiveThrall(input.playerId)
      return thrallSchema.parse(thrall)
    })
})

