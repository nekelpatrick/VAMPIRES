import { z } from 'zod'
import { publicProcedure, router } from '../../trpc/core'
import { clanService } from './clan.service'
import {
  clanTypeSchema,
  joinClanRequestSchema,
  leaveClanRequestSchema
} from './clan.schema'

export const clanRouter = router({
  getAll: publicProcedure.query(() => {
    return clanService.getAllClans()
  }),

  get: publicProcedure
    .input(z.object({ clanId: clanTypeSchema }))
    .query(({ input }) => {
      return clanService.getClan(input.clanId)
    }),

  getBonuses: publicProcedure
    .input(z.object({ clanId: clanTypeSchema }))
    .query(({ input }) => {
      return clanService.getClanBonuses(input.clanId)
    }),

  getPlayerClan: publicProcedure
    .input(z.object({ playerId: z.string().min(1) }))
    .query(async ({ input }) => {
      return clanService.getPlayerClan(input.playerId)
    }),

  getPlayerBonuses: publicProcedure
    .input(z.object({ playerId: z.string().min(1) }))
    .query(async ({ input }) => {
      return clanService.getPlayerClanBonuses(input.playerId)
    }),

  join: publicProcedure.input(joinClanRequestSchema).mutation(async ({ input }) => {
    return clanService.joinClan(input.playerId, input.clanId)
  }),

  leave: publicProcedure.input(leaveClanRequestSchema).mutation(async ({ input }) => {
    return clanService.leaveClan(input.playerId)
  }),

  getLeaderboard: publicProcedure
    .input(z.object({ clanId: clanTypeSchema }))
    .query(({ input }) => {
      return clanService.getClanLeaderboard(input.clanId)
    })
})

