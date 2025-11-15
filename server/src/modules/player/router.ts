import { z } from 'zod'

import { procedure, router } from '../../trpc/core'
import { playerSchema } from './player.schema'
import { playerService } from './player.service'

export const playerRouter = router({
  getProfile: procedure
    .input(z.object({ playerId: z.string().min(1) }))
    .output(playerSchema)
    .query(async ({ input }) => {
      const profile = await playerService.getProfile(input.playerId)
      return playerSchema.parse(profile)
    })
})

