import assert from 'node:assert'
import { describe, it } from 'node:test'

import {
  resolveBattle,
  generateEnemiesForWave,
  createSeed,
  type CombatStats
} from '../src/modules/battle/combat-engine'

const THRALL_STATS: CombatStats = {
  maxHealth: 150,
  attack: 25,
  defense: 10,
  speed: 1.2
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

describe('Combat Engine', () => {
  describe('Determinism', () => {
    it('should produce identical results with the same seed', () => {
      const seed = 12345
      const rng1 = mulberry32(seed)
      const rng2 = mulberry32(seed)

      const enemies1 = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng1)
      const enemies2 = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng2)

      const result1 = resolveBattle(THRALL_STATS, '[WEREWOLF]', enemies1, seed)
      const result2 = resolveBattle(THRALL_STATS, '[WEREWOLF]', enemies2, seed)

      assert.strictEqual(result1.result.winner, result2.result.winner)
      assert.strictEqual(result1.result.totalTicks, result2.result.totalTicks)
      assert.strictEqual(result1.result.damageDealt, result2.result.damageDealt)
      assert.strictEqual(result1.result.damageTaken, result2.result.damageTaken)
      assert.strictEqual(result1.events.length, result2.events.length)
    })

    it('should produce different results with different seeds', () => {
      const seed1 = 12345
      const seed2 = 67890
      const rng1 = mulberry32(seed1)
      const rng2 = mulberry32(seed2)

      const enemies1 = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng1)
      const enemies2 = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng2)

      const result1 = resolveBattle(THRALL_STATS, '[WEREWOLF]', enemies1, seed1)
      const result2 = resolveBattle(THRALL_STATS, '[WEREWOLF]', enemies2, seed2)

      const isDifferent =
        result1.result.totalTicks !== result2.result.totalTicks ||
        result1.result.damageDealt !== result2.result.damageDealt

      assert.ok(isDifferent, 'Different seeds should produce different results')
    })
  })

  describe('Battle Resolution', () => {
    it('should resolve a battle and return a valid result', () => {
      const seed = 42
      const rng = mulberry32(seed)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      const outcome = resolveBattle(THRALL_STATS, '[WEREWOLF]', enemies, seed)

      assert.ok(['player', 'enemy', 'draw'].includes(outcome.result.winner))
      assert.ok(outcome.result.totalTicks > 0)
      assert.ok(outcome.events.length > 0)
      assert.strictEqual(outcome.seed, seed)
    })

    it('should include BATTLE_START and BATTLE_END events', () => {
      const seed = 123
      const rng = mulberry32(seed)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      const outcome = resolveBattle(THRALL_STATS, '[WEREWOLF]', enemies, seed)

      const startEvent = outcome.events.find((e) => e.type === 'BATTLE_START')
      const endEvent = outcome.events.find((e) => e.type === 'BATTLE_END')

      assert.ok(startEvent, 'Should have BATTLE_START event')
      assert.ok(endEvent, 'Should have BATTLE_END event')
      assert.strictEqual(startEvent?.tick, 0)
    })

    it('should record damage events', () => {
      const seed = 456
      const rng = mulberry32(seed)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      const outcome = resolveBattle(THRALL_STATS, '[WEREWOLF]', enemies, seed)

      const damageEvents = outcome.events.filter((e) => e.type === 'DAMAGE')
      assert.ok(damageEvents.length > 0, 'Should have damage events')
      assert.ok(damageEvents.every((e) => e.value !== undefined && e.value > 0))
    })

    it('should record death events when entities die', () => {
      const seed = 789
      const rng = mulberry32(seed)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      const outcome = resolveBattle(THRALL_STATS, '[WEREWOLF]', enemies, seed)

      if (outcome.result.enemiesKilled > 0) {
        const deathEvents = outcome.events.filter((e) => e.type === 'DEATH')
        assert.ok(deathEvents.length > 0, 'Should have death events')
      }
    })
  })

  describe('Enemy Generation', () => {
    it('should generate the correct number of enemies for wave 1', () => {
      const rng = mulberry32(1)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      assert.strictEqual(enemies.length, 3)
    })

    it('should scale enemy count with wave number', () => {
      const rng1 = mulberry32(1)
      const rng10 = mulberry32(1)

      const wave1Enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng1)
      const wave10Enemies = generateEnemiesForWave(10, BASE_ENEMY_STATS, rng10)

      assert.ok(wave10Enemies.length > wave1Enemies.length)
    })

    it('should scale enemy stats with wave number', () => {
      const rng1 = mulberry32(1)
      const rng5 = mulberry32(1)

      const wave1Enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng1)
      const wave5Enemies = generateEnemiesForWave(5, BASE_ENEMY_STATS, rng5)

      const wave1Stats = wave1Enemies[0].stats
      const wave5Stats = wave5Enemies[0].stats

      assert.ok(wave5Stats.maxHealth > wave1Stats.maxHealth)
      assert.ok(wave5Stats.attack > wave1Stats.attack)
    })

    it('should cap enemy count at 10', () => {
      const rng = mulberry32(1)
      const enemies = generateEnemiesForWave(100, BASE_ENEMY_STATS, rng)

      assert.ok(enemies.length <= 10)
    })
  })

  describe('Seed Generation', () => {
    it('should generate valid seeds', () => {
      const seed = createSeed()
      assert.ok(seed > 0)
      assert.ok(seed < 2147483647)
      assert.ok(Number.isInteger(seed))
    })
  })
})

