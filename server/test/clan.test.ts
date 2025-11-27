import assert from 'node:assert'
import { describe, it, beforeEach } from 'node:test'

import {
  clanService,
  CLAN_DEFINITIONS,
  type ClanType
} from '../src/modules/clan'
import {
  resolveBattle,
  applyClanBonuses,
  generateEnemiesForWave,
  type CombatStats
} from '../src/modules/battle/combat-engine'

const THRALL_STATS: CombatStats = {
  maxHealth: 150,
  attack: 25,
  defense: 10,
  speed: 1.0,
  critChance: 0.05,
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

describe('Clan Module', () => {
  describe('Clan Definitions', () => {
    it('should have all 3 clans defined', () => {
      const clans: ClanType[] = ['CLAN_NOCTURNUM', 'CLAN_SABLEHEART', 'CLAN_ECLIPSA']

      for (const clan of clans) {
        assert.ok(CLAN_DEFINITIONS[clan], `${clan} should be defined`)
      }
    })

    it('should have correct bonuses for Nocturnum', () => {
      const nocturnum = CLAN_DEFINITIONS.CLAN_NOCTURNUM

      assert.strictEqual(nocturnum.bonuses.lifestealBonus, 0.05)
      assert.strictEqual(nocturnum.bonuses.attackSpeedBonus, 0)
      assert.strictEqual(nocturnum.bonuses.bleedChanceBonus, 0)
    })

    it('should have correct bonuses for Sableheart', () => {
      const sableheart = CLAN_DEFINITIONS.CLAN_SABLEHEART

      assert.strictEqual(sableheart.bonuses.lifestealBonus, 0)
      assert.strictEqual(sableheart.bonuses.attackSpeedBonus, 0.1)
      assert.strictEqual(sableheart.bonuses.bleedChanceBonus, 0)
    })

    it('should have correct bonuses for Eclipsa', () => {
      const eclipsa = CLAN_DEFINITIONS.CLAN_ECLIPSA

      assert.strictEqual(eclipsa.bonuses.lifestealBonus, 0)
      assert.strictEqual(eclipsa.bonuses.attackSpeedBonus, 0)
      assert.strictEqual(eclipsa.bonuses.bleedChanceBonus, 0.15)
    })
  })

  describe('Clan Service', () => {
    it('should get all clans', () => {
      const clans = clanService.getAllClans()

      assert.strictEqual(clans.length, 3)
      assert.ok(clans.some((c) => c.id === 'CLAN_NOCTURNUM'))
      assert.ok(clans.some((c) => c.id === 'CLAN_SABLEHEART'))
      assert.ok(clans.some((c) => c.id === 'CLAN_ECLIPSA'))
    })

    it('should get specific clan by id', () => {
      const nocturnum = clanService.getClan('CLAN_NOCTURNUM')

      assert.strictEqual(nocturnum.id, 'CLAN_NOCTURNUM')
      assert.strictEqual(nocturnum.name, 'Clan Nocturnum')
      assert.ok(nocturnum.description.includes('blood rituals'))
    })

    it('should get clan bonuses', () => {
      const bonuses = clanService.getClanBonuses('CLAN_SABLEHEART')

      assert.strictEqual(bonuses.attackSpeedBonus, 0.1)
    })

    it('should join a clan', async () => {
      const playerId = 'test-player-clan-1'

      const membership = await clanService.joinClan(playerId, 'CLAN_NOCTURNUM')

      assert.strictEqual(membership.playerId, playerId)
      assert.strictEqual(membership.clanId, 'CLAN_NOCTURNUM')
      assert.ok(membership.joinedAt instanceof Date)
    })

    it('should get player clan after joining', async () => {
      const playerId = 'test-player-clan-2'

      await clanService.joinClan(playerId, 'CLAN_ECLIPSA')
      const membership = await clanService.getPlayerClan(playerId)

      assert.ok(membership)
      assert.strictEqual(membership.clanId, 'CLAN_ECLIPSA')
    })

    it('should switch clans when joining a new one', async () => {
      const playerId = 'test-player-clan-3'

      await clanService.joinClan(playerId, 'CLAN_NOCTURNUM')
      await clanService.joinClan(playerId, 'CLAN_SABLEHEART')

      const membership = await clanService.getPlayerClan(playerId)

      assert.ok(membership)
      assert.strictEqual(membership.clanId, 'CLAN_SABLEHEART')
    })

    it('should leave a clan', async () => {
      const playerId = 'test-player-clan-4'

      await clanService.joinClan(playerId, 'CLAN_NOCTURNUM')
      const left = await clanService.leaveClan(playerId)

      assert.strictEqual(left, true)

      const membership = await clanService.getPlayerClan(playerId)
      assert.strictEqual(membership, null)
    })

    it('should get player clan bonuses', async () => {
      const playerId = 'test-player-clan-5'

      await clanService.joinClan(playerId, 'CLAN_ECLIPSA')
      const bonuses = await clanService.getPlayerClanBonuses(playerId)

      assert.ok(bonuses)
      assert.strictEqual(bonuses.bleedChanceBonus, 0.15)
    })

    it('should return null bonuses for player without clan', async () => {
      const playerId = 'test-player-no-clan'

      const bonuses = await clanService.getPlayerClanBonuses(playerId)

      assert.strictEqual(bonuses, null)
    })
  })

  describe('Clan Bonuses in Combat', () => {
    it('should apply Nocturnum lifesteal bonus to stats', () => {
      const modified = applyClanBonuses(THRALL_STATS, 'CLAN_NOCTURNUM')

      assert.strictEqual(modified.lifestealPercent, 0.05)
      assert.strictEqual(modified.speed, THRALL_STATS.speed)
    })

    it('should apply Sableheart attack speed bonus to stats', () => {
      const modified = applyClanBonuses(THRALL_STATS, 'CLAN_SABLEHEART')

      assert.strictEqual(modified.speed, THRALL_STATS.speed * 1.1)
      assert.strictEqual(modified.lifestealPercent, 0)
    })

    it('should apply Eclipsa bleed chance bonus to stats', () => {
      const modified = applyClanBonuses(THRALL_STATS, 'CLAN_ECLIPSA')

      assert.strictEqual(modified.bleedChance, 0.15)
      assert.strictEqual(modified.speed, THRALL_STATS.speed)
    })

    it('should not modify stats when no clan', () => {
      const modified = applyClanBonuses(THRALL_STATS, null)

      assert.deepStrictEqual(modified, THRALL_STATS)
    })

    it('should use clan bonuses in battle resolution', () => {
      const seed = 42
      const rng = mulberry32(seed)
      const enemies = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng)

      const outcomeNoClan = resolveBattle(
        THRALL_STATS,
        '[WEREWOLF]',
        enemies,
        seed,
        1000,
        [],
        null
      )

      const rng2 = mulberry32(seed)
      const enemies2 = generateEnemiesForWave(1, BASE_ENEMY_STATS, rng2)

      const outcomeSableheart = resolveBattle(
        THRALL_STATS,
        '[WEREWOLF]',
        enemies2,
        seed,
        1000,
        [],
        'CLAN_SABLEHEART'
      )

      assert.ok(
        outcomeSableheart.result.totalTicks <= outcomeNoClan.result.totalTicks,
        'Sableheart speed bonus should result in faster or equal battle'
      )
    })
  })
})

