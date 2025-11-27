import type { Ability, AbilityType, ThrallAbility } from './ability.schema'
import { ABILITY_DEFINITIONS } from './ability.schema'
import { generateId } from '../../lib/id'

const thrallAbilities: Map<string, ThrallAbility[]> = new Map()

export class AbilityService {
  getAbilityDefinition(type: AbilityType): Ability {
    const def = ABILITY_DEFINITIONS[type]
    return {
      ...def,
      id: `ability-${type.toLowerCase()}`
    }
  }

  getAllAbilityDefinitions(): Ability[] {
    return Object.keys(ABILITY_DEFINITIONS).map((type) =>
      this.getAbilityDefinition(type as AbilityType)
    )
  }

  async getThrallAbilities(thrallId: string): Promise<Ability[]> {
    const assignments = thrallAbilities.get(thrallId) || []
    return assignments.map((ta) => {
      const type = ta.abilityId.replace('ability-', '').toUpperCase() as AbilityType
      return this.getAbilityDefinition(type)
    })
  }

  async grantAbility(thrallId: string, type: AbilityType): Promise<ThrallAbility> {
    const abilityId = `ability-${type.toLowerCase()}`
    const existing = thrallAbilities.get(thrallId) || []

    const alreadyHas = existing.some((a) => a.abilityId === abilityId)
    if (alreadyHas) {
      return existing.find((a) => a.abilityId === abilityId)!
    }

    const assignment: ThrallAbility = {
      thrallId,
      abilityId,
      unlockedAt: new Date()
    }

    thrallAbilities.set(thrallId, [...existing, assignment])
    return assignment
  }

  async revokeAbility(thrallId: string, type: AbilityType): Promise<boolean> {
    const abilityId = `ability-${type.toLowerCase()}`
    const existing = thrallAbilities.get(thrallId) || []
    const filtered = existing.filter((a) => a.abilityId !== abilityId)

    if (filtered.length === existing.length) {
      return false
    }

    thrallAbilities.set(thrallId, filtered)
    return true
  }

  getDefaultAbilitiesForLevel(level: number): AbilityType[] {
    const abilities: AbilityType[] = []

    if (level >= 1) abilities.push('LIFESTEAL')
    if (level >= 5) abilities.push('BLEED')
    if (level >= 10) abilities.push('RAGE')
    if (level >= 15) abilities.push('STUN')
    if (level >= 20) abilities.push('HOWL')

    return abilities
  }
}

export const abilityService = new AbilityService()

