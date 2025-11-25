import { TRPCError } from '@trpc/server'

import { procedure, router } from '../../trpc/core'
import {
  getDailyQuestsRequestSchema,
  updateQuestProgressRequestSchema,
  claimQuestRequestSchema,
  questProgressSchema,
  claimQuestResponseSchema
} from './quest.schema'
import { questService } from './quest.service'

export const questRouter = router({
  getDailyQuests: procedure
    .input(getDailyQuestsRequestSchema)
    .output(questProgressSchema.array())
    .query(async ({ input }) => {
      const quests = await questService.getDailyQuests(input.playerId)
      return quests
    }),

  updateProgress: procedure
    .input(updateQuestProgressRequestSchema)
    .mutation(async ({ input }) => {
      const quest = await questService.updateProgress(
        input.playerId,
        input.questType,
        input.value
      )

      if (!quest) {
        return { updated: false, quest: null }
      }

      return { updated: true, quest }
    }),

  claimQuest: procedure
    .input(claimQuestRequestSchema)
    .output(claimQuestResponseSchema.nullable())
    .mutation(async ({ input }) => {
      const result = await questService.claimQuest(
        input.playerId,
        input.questId,
        input.watchedAd
      )

      if (!result) {
        throw new TRPCError({
          code: 'BAD_REQUEST',
          message: 'Quest not found, not completed, or already claimed'
        })
      }

      return result
    })
})

