import { TRPCError } from '@trpc/server'

import { procedure, router } from '../../trpc/core'
import {
  joinQueueRequestSchema,
  leaveQueueRequestSchema,
  pvpBattleRequestSchema,
  getPvpHistoryRequestSchema
} from './pvp.schema'
import { pvpService } from './pvp.service'

export const pvpRouter = router({
  joinQueue: procedure
    .input(joinQueueRequestSchema)
    .mutation(async ({ input }) => {
      const result = await pvpService.joinQueue(input.playerId, input.thrallId)

      if (!result.success) {
        throw new TRPCError({
          code: 'BAD_REQUEST',
          message: result.error
        })
      }

      return {
        success: true,
        matchFound: result.matchFound ?? null
      }
    }),

  leaveQueue: procedure
    .input(leaveQueueRequestSchema)
    .mutation(async ({ input }) => {
      const success = await pvpService.leaveQueue(input.playerId)
      return { success }
    }),

  getQueueStatus: procedure
    .input(leaveQueueRequestSchema)
    .query(({ input }) => {
      return pvpService.getQueueStatus(input.playerId)
    }),

  resolveBattle: procedure
    .input(pvpBattleRequestSchema)
    .mutation(async ({ input }) => {
      const result = await pvpService.resolvePvpBattle(input.matchId)

      if (!result) {
        throw new TRPCError({
          code: 'NOT_FOUND',
          message: 'Match not found or already resolved'
        })
      }

      return result
    }),

  getMatch: procedure
    .input(pvpBattleRequestSchema)
    .query(async ({ input }) => {
      const match = await pvpService.getMatch(input.matchId)

      if (!match) {
        throw new TRPCError({
          code: 'NOT_FOUND',
          message: 'Match not found'
        })
      }

      return match
    }),

  getHistory: procedure
    .input(getPvpHistoryRequestSchema)
    .query(async ({ input }) => {
      return pvpService.getPvpHistory(input.playerId, input.limit, input.offset)
    })
})

