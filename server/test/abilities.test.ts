import assert from 'node:assert'
import { describe, it, beforeEach } from 'node:test'

import {
  abilityService,
  ABILITY_DEFINITIONS,
  type AbilityType
} from '../src/modules/abilities'
import {
  resolveBattle,
  generateEnemiesForWave,
  type CombatStats
} from '../src/modules/battle/combat-engine'

const THRALL_STATS: CombatStats = {
  maxHealth: 150,
  attack: 25,
  defense: 10,
  speed: 1.2,
  critChance: 0.1,
  lifestealPercent: 0,
  bleedChance: 0
}

const BASE_ENEMY_STATS: CombatStats = {
  maxHealth: 90,
  attack: 14,
  defense: 4,
  speed: 1.0
}

function mulberry32(seed: number): () => number {
  return () => {
    let t = (seed += 0x6d2b79f5)
    t = Math.imul(t ^ (t >>> 15), t | 1)
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61)
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296
  }
}

describe('Abilities Module', () => {
  describe('Ability Definitions', () => {
    it('should have all 5 ability types defined', () => {
      const types: AbilityType[] = ['LIFESTEAL', 'BLEED', 'STUN', 'RAGE', 'HOWL']

      for (const type of types) {
        assert.ok(ABILITY_DEFINITIONS[type], `${type} should be defined`)
      }
    })

    it('should have valid magnitude values', () => {
      for (const [type, def] of Object.entries(ABILITY_DEFINITIONS)) {
        assert.ok(
          typeof def.magnitude === 'number',
          `${type} should have numeric magnitude`
        )
      }
    })

    it('should have valid trigger types', () => {
      const validTriggers = ['ON_ATTACK', 'ON_HIT', 'ON_KILL', 'ON_LOW_HEALTH', 'ACTIVE']

      for (const [type, def] of Object.entries(ABILITY_DEFINITIONS)) {
        assert.ok(
          validTriggers.includes(def.trigger),
          `${type} should have valid trigger`
        )
      }
    })
  })

  describe('Ability Service', () => {
    it('should get ability definition by type', () => {
      const lifesteal = abilityService.getAbilityDefinition('LIFESTEAL')

      assert.ok(lifesteal)
      assert.strictEqual(lifesteal.type, 'LIFESTEAL')
      assert.ok(lifesteal.id.includes('lifesteal'))
    })

    it('should get all ability definitions', () => {
      const all = abilityService.getAllAbilityDefinitions()

      assert.strictEqual(all.length, 5)
      assert.ok(all.some((a) => a.type === 'LIFESTEAL'))
      assert.ok(all.some((a) => a.type === 'BLEED'))
      assert.ok(all.some((a) => a.type === 'STUN'))
      assert.ok(all.some((a) => a.type === 'RAGE'))
      assert.ok(all.some((a) => a.type === 'HOWL'))
    })

    it('should return default abilities based on level', () => {
      const level1 = abilityService.getDefaultAbilitiesForLevel(1)
      const level5 = abilityService.getDefaultAbilitiesForLevel(5)
      const level20 = abilityService.getDefaultAbilitiesForLevel(20)

      assert.ok(level1.includes('LIFESTEAL'))
      assert.ok(!level1.includes('BLEED'))

      assert.ok(level5.includes('LIFESTEAL'))
      assert.ok(level5.includes('BLEED'))

      assert.ok(level20.includes('HOWL'))
      assert.strictEqual(level20.length, 5)
    })

    it('should grant and retrieve thrall abilities', async () => {
      const thrallId = 'test-thrall-1'

      await abilityService.grantAbility(thrallId, 'LIFESTEAL')
      await abilityService.grantAbility(thrallId, 'BLEED')

      const abilities = await abilityService.getThrallAbilities(thrallId)

      assert.strictEqual(abilities.length, 2)
      assert.ok(abilities.some((a) => a.type === 'LIFESTEAL'))
      assert.ok(abilities.some((a) => a.type === 'BLEED'))
    })

    it('should not duplicate abilities when granting same type', async () => {
      const thrallId = 'test-thrall-2'

      await abilityService.grantAbility(thrallId, 'STUN')
      await abilityService.grantAbility(thrallId, 'STUN')

      const abilities = await abilityService.getThrallAbilities(thrallId)

      assert.strictEqual(abilities.length, 1)
    })

    it('should revoke abilities', async () => {
      const thrallId = 'test-thrall-3'

      await abilityService.grantAbility(thrallId, 'RAGE')
      const revoked = await abilityService.revokeAbility(thrallId, 'RAGE')

      assert.strictEqual(revoked, true)

      const abilities = await abilityService.getThrallAbilities(thrallId)
      assert.ok(!abilities.some((a) => a.type === 'RAGE'))
    })
  })

  describe('Combat with Abilities', () => {
    it('should trigger ABILITY_TRIGGER events in battle', () => {
      const seed = 42
      const rng = mulberry32(seed)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      const lifestealAbility = abilityService.getAbilityDefinition('LIFESTEAL')

      const outcome = resolveBattle(
        THRALL_STATS,
        '[WEREWOLF]',
        enemies,
        seed,
        1000,
        [lifestealAbility]
      )

      const abilityEvents = outcome.events.filter((e) => e.type === 'ABILITY_TRIGGER')
      assert.ok(abilityEvents.length > 0, 'Should have ability trigger events')
    })

    it('should apply lifesteal healing', () => {
      const seed = 123
      const rng = mulberry32(seed)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      const lifestealAbility = abilityService.getAbilityDefinition('LIFESTEAL')

      const outcome = resolveBattle(
        { ...THRALL_STATS, lifestealPercent: 0.1 },
        '[WEREWOLF]',
        enemies,
        seed,
        1000,
        [lifestealAbility]
      )

      const healEvents = outcome.events.filter((e) => e.type === 'HEAL')
      assert.ok(healEvents.length > 0, 'Should have heal events from lifesteal')
    })

    it('should apply bleed status effect', () => {
      const seed = 456
      const rng = mulberry32(seed)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      const bleedAbility = {
        ...abilityService.getAbilityDefinition('BLEED'),
        chance: 1
      }

      const outcome = resolveBattle(
        THRALL_STATS,
        '[WEREWOLF]',
        enemies,
        seed,
        1000,
        [bleedAbility]
      )

      const statusEvents = outcome.events.filter((e) => e.type === 'STATUS_APPLIED')
      const bleedApplied = statusEvents.some(
        (e) => e.data?.effectType === 'BLEED'
      )

      assert.ok(bleedApplied, 'Should apply bleed status')
    })

    it('should process bleed damage over time', () => {
      const seed = 789
      const rng = mulberry32(seed)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      const bleedAbility = {
        ...abilityService.getAbilityDefinition('BLEED'),
        chance: 1
      }

      const outcome = resolveBattle(
        THRALL_STATS,
        '[WEREWOLF]',
        enemies,
        seed,
        1000,
        [bleedAbility]
      )

      const tickEvents = outcome.events.filter((e) => e.type === 'STATUS_TICK')
      const bleedTicks = tickEvents.filter((e) => e.data?.effectType === 'BLEED')

      assert.ok(bleedTicks.length > 0, 'Should have bleed tick events')
    })

    it('should trigger critical hits based on critChance', () => {
      const seed = 1111
      const rng = mulberry32(seed)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      const outcome = resolveBattle(
        { ...THRALL_STATS, critChance: 0.5 },
        '[WEREWOLF]',
        enemies,
        seed,
        1000,
        []
      )

      const critEvents = outcome.events.filter((e) => e.type === 'CRITICAL')
      assert.ok(critEvents.length > 0, 'Should have critical hit events')
    })
  })
})

