import { TRPCError } from '@trpc/server'

import { procedure, router } from '../../trpc/core'
import {
  startBattleRequestSchema,
  getBattleRequestSchema,
  getBattlesByPlayerRequestSchema,
  startBattleResponseSchema,
  battleRecordSchema
} from './battle.schema'
import { battleService } from './battle.service'

export const battleRouter = router({
  startBattle: procedure
    .input(startBattleRequestSchema)
    .output(startBattleResponseSchema)
    .mutation(async ({ input }) => {
      const response = await battleService.startBattle(input)
      return response
    }),

  getBattle: procedure
    .input(getBattleRequestSchema)
    .output(battleRecordSchema.nullable())
    .query(async ({ input }) => {
      const battle = await battleService.getBattle(input.battleId)
      return battle
    }),

  getBattlesByPlayer: procedure
    .input(getBattlesByPlayerRequestSchema)
    .output(battleRecordSchema.array())
    .query(async ({ input }) => {
      const battles = await battleService.getBattlesByPlayer(
        input.playerId,
        input.limit,
        input.offset
      )
      return battles
    }),

  replayBattle: procedure
    .input(getBattleRequestSchema)
    .query(async ({ input }) => {
      const outcome = await battleService.replayBattle(input.battleId)

      if (!outcome) {
        throw new TRPCError({
          code: 'NOT_FOUND',
          message: 'Battle not found'
        })
      }

      return {
        result: outcome.result,
        events: outcome.events,
        seed: outcome.seed
      }
    })
})

