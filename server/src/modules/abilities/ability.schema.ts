import { z } from 'zod'

export const abilityTypeSchema = z.enum([
  'LIFESTEAL',
  'BLEED',
  'STUN',
  'RAGE',
  'HOWL'
])

export type AbilityType = z.infer<typeof abilityTypeSchema>

export const abilityTriggerSchema = z.enum([
  'ON_ATTACK',
  'ON_HIT',
  'ON_KILL',
  'ON_LOW_HEALTH',
  'ACTIVE'
])

export type AbilityTrigger = z.infer<typeof abilityTriggerSchema>

export const abilitySchema = z.object({
  id: z.string().min(1),
  type: abilityTypeSchema,
  trigger: abilityTriggerSchema,
  chance: z.number().min(0).max(1).default(1),
  magnitude: z.number().positive(),
  duration: z.number().nonnegative().default(0),
  cooldown: z.number().nonnegative().default(0)
})

export type Ability = z.infer<typeof abilitySchema>

export const statusEffectSchema = z.object({
  id: z.string().min(1),
  type: abilityTypeSchema,
  sourceId: z.string().min(1),
  targetId: z.string().min(1),
  magnitude: z.number(),
  remainingDuration: z.number().nonnegative(),
  ticksApplied: z.number().int().nonnegative().default(0)
})

export type StatusEffect = z.infer<typeof statusEffectSchema>

export const thrallAbilitySchema = z.object({
  thrallId: z.string().min(1),
  abilityId: z.string().min(1),
  unlockedAt: z.date().optional()
})

export type ThrallAbility = z.infer<typeof thrallAbilitySchema>

export const ABILITY_DEFINITIONS: Record<AbilityType, Omit<Ability, 'id'>> = {
  LIFESTEAL: {
    type: 'LIFESTEAL',
    trigger: 'ON_ATTACK',
    chance: 1,
    magnitude: 0.1,
    duration: 0,
    cooldown: 0
  },
  BLEED: {
    type: 'BLEED',
    trigger: 'ON_HIT',
    chance: 0.25,
    magnitude: 5,
    duration: 3,
    cooldown: 0
  },
  STUN: {
    type: 'STUN',
    trigger: 'ON_HIT',
    chance: 0.1,
    magnitude: 0,
    duration: 1,
    cooldown: 5
  },
  RAGE: {
    type: 'RAGE',
    trigger: 'ON_LOW_HEALTH',
    chance: 1,
    magnitude: 0.5,
    duration: 5,
    cooldown: 30
  },
  HOWL: {
    type: 'HOWL',
    trigger: 'ACTIVE',
    chance: 1,
    magnitude: 0.2,
    duration: 3,
    cooldown: 15
  }
}

