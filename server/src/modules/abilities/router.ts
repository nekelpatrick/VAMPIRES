import { z } from 'zod'
import { publicProcedure, router } from '../../trpc/core'
import { abilityService } from './ability.service'
import { abilityTypeSchema } from './ability.schema'

export const abilitiesRouter = router({
  getDefinitions: publicProcedure.query(() => {
    return abilityService.getAllAbilityDefinitions()
  }),

  getDefinition: publicProcedure
    .input(z.object({ type: abilityTypeSchema }))
    .query(({ input }) => {
      return abilityService.getAbilityDefinition(input.type)
    }),

  getThrallAbilities: publicProcedure
    .input(z.object({ thrallId: z.string().min(1) }))
    .query(async ({ input }) => {
      return abilityService.getThrallAbilities(input.thrallId)
    }),

  grantAbility: publicProcedure
    .input(
      z.object({
        thrallId: z.string().min(1),
        type: abilityTypeSchema
      })
    )
    .mutation(async ({ input }) => {
      return abilityService.grantAbility(input.thrallId, input.type)
    }),

  revokeAbility: publicProcedure
    .input(
      z.object({
        thrallId: z.string().min(1),
        type: abilityTypeSchema
      })
    )
    .mutation(async ({ input }) => {
      return abilityService.revokeAbility(input.thrallId, input.type)
    }),

  getDefaultForLevel: publicProcedure
    .input(z.object({ level: z.number().int().positive() }))
    .query(({ input }) => {
      return abilityService.getDefaultAbilitiesForLevel(input.level)
    })
})

