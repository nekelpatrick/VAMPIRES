import { strictEqual, ok, notStrictEqual } from 'node:assert/strict'
import { describe, it, beforeEach } from 'node:test'

import { ThrallService } from '../src/modules/thrall/thrall.service'

describe('ThrallService', () => {
  let thrallService: ThrallService

  beforeEach(() => {
    thrallService = new ThrallService()
  })

  describe('createThrall', () => {
    it('creates a werewolf with correct base stats', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'owner-1',
        archetype: '[WEREWOLF]'
      })

      strictEqual(thrall.ownerId, 'owner-1')
      strictEqual(thrall.archetype, '[WEREWOLF]')
      strictEqual(thrall.level, 1)
      strictEqual(thrall.xp, 0)
      strictEqual(thrall.maxHp, 150)
      strictEqual(thrall.hp, 150)
      strictEqual(thrall.attack, 25)
      strictEqual(thrall.defense, 10)
      strictEqual(thrall.speed, 1.2)
      strictEqual(thrall.status, 'ACTIVE')
    })

    it('creates a thrall with correct base stats', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'owner-2',
        archetype: '[THRALL]'
      })

      strictEqual(thrall.archetype, '[THRALL]')
      strictEqual(thrall.maxHp, 100)
      strictEqual(thrall.attack, 20)
      strictEqual(thrall.defense, 8)
      strictEqual(thrall.speed, 1.0)
    })

    it('generates unique IDs for each thrall', async () => {
      const thrall1 = await thrallService.createThrall({ ownerId: 'o1', archetype: '[WEREWOLF]' })
      const thrall2 = await thrallService.createThrall({ ownerId: 'o2', archetype: '[WEREWOLF]' })

      notStrictEqual(thrall1.id, thrall2.id)
    })

    it('initializes PvP and death counters to zero', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'owner-3',
        archetype: '[WEREWOLF]'
      })

      strictEqual(thrall.pvpWins, 0)
      strictEqual(thrall.pvpLosses, 0)
      strictEqual(thrall.deathCount, 0)
    })
  })

  describe('getThrall', () => {
    it('retrieves a thrall by ID', async () => {
      const created = await thrallService.createThrall({
        ownerId: 'get-owner',
        archetype: '[WEREWOLF]'
      })

      const retrieved = await thrallService.getThrall(created.id)

      ok(retrieved)
      strictEqual(retrieved.id, created.id)
      strictEqual(retrieved.ownerId, 'get-owner')
    })

    it('returns null for non-existent ID', async () => {
      const result = await thrallService.getThrall('non-existent-id')
      strictEqual(result, null)
    })
  })

  describe('getThrallByOwner', () => {
    it('retrieves a thrall by owner ID', async () => {
      const created = await thrallService.createThrall({
        ownerId: 'owner-by-id',
        archetype: '[WEREWOLF]'
      })

      const retrieved = await thrallService.getThrallByOwner('owner-by-id')

      ok(retrieved)
      strictEqual(retrieved.id, created.id)
    })

    it('returns null for non-existent owner', async () => {
      const result = await thrallService.getThrallByOwner('no-owner')
      strictEqual(result, null)
    })
  })

  describe('getOrCreateThrall', () => {
    it('creates a thrall if none exists for owner', async () => {
      const thrall = await thrallService.getOrCreateThrall('new-owner')

      ok(thrall)
      strictEqual(thrall.ownerId, 'new-owner')
      strictEqual(thrall.archetype, '[WEREWOLF]')
    })

    it('returns existing thrall if one exists', async () => {
      const created = await thrallService.createThrall({
        ownerId: 'existing-owner',
        archetype: '[WEREWOLF]'
      })

      const retrieved = await thrallService.getOrCreateThrall('existing-owner')

      strictEqual(retrieved.id, created.id)
    })
  })

  describe('updateStatus', () => {
    it('updates thrall status', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'status-owner',
        archetype: '[WEREWOLF]'
      })

      const updated = await thrallService.updateStatus(thrall.id, 'REVIVING')

      ok(updated)
      strictEqual(updated.status, 'REVIVING')
    })

    it('increments death count when status is DEAD', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'death-owner',
        archetype: '[WEREWOLF]'
      })

      strictEqual(thrall.deathCount, 0)

      const updated = await thrallService.updateStatus(thrall.id, 'DEAD')

      ok(updated)
      strictEqual(updated.status, 'DEAD')
      strictEqual(updated.deathCount, 1)
    })

    it('sets diedAt and reviveAt when provided', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'dates-owner',
        archetype: '[WEREWOLF]'
      })

      const diedAt = new Date()
      const reviveAt = new Date(Date.now() + 3600000)

      const updated = await thrallService.updateStatus(thrall.id, 'DEAD', diedAt, reviveAt)

      ok(updated)
      strictEqual(updated.diedAt?.getTime(), diedAt.getTime())
      strictEqual(updated.reviveAt?.getTime(), reviveAt.getTime())
    })

    it('returns null for non-existent thrall', async () => {
      const result = await thrallService.updateStatus('fake-id', 'DEAD')
      strictEqual(result, null)
    })
  })

  describe('healThrall', () => {
    it('restores HP to max and sets status to ACTIVE', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'heal-owner',
        archetype: '[WEREWOLF]'
      })

      await thrallService.updateStatus(thrall.id, 'DEAD', new Date(), new Date())

      const healed = await thrallService.healThrall(thrall.id)

      ok(healed)
      strictEqual(healed.hp, healed.maxHp)
      strictEqual(healed.status, 'ACTIVE')
      strictEqual(healed.diedAt, null)
      strictEqual(healed.reviveAt, null)
    })

    it('returns null for non-existent thrall', async () => {
      const result = await thrallService.healThrall('fake-id')
      strictEqual(result, null)
    })
  })

  describe('recordPvpResult', () => {
    it('increments wins on victory', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'pvp-win-owner',
        archetype: '[WEREWOLF]'
      })

      const updated = await thrallService.recordPvpResult(thrall.id, true)

      ok(updated)
      strictEqual(updated.pvpWins, 1)
      strictEqual(updated.pvpLosses, 0)
    })

    it('increments losses on defeat', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'pvp-loss-owner',
        archetype: '[WEREWOLF]'
      })

      const updated = await thrallService.recordPvpResult(thrall.id, false)

      ok(updated)
      strictEqual(updated.pvpWins, 0)
      strictEqual(updated.pvpLosses, 1)
    })

    it('returns null for non-existent thrall', async () => {
      const result = await thrallService.recordPvpResult('fake-id', true)
      strictEqual(result, null)
    })
  })

  describe('calculatePowerScore', () => {
    it('calculates power score correctly', () => {
      const thrall = {
        id: 'calc-id',
        ownerId: 'calc-owner',
        archetype: '[WEREWOLF]' as const,
        level: 1,
        xp: 0,
        hp: 150,
        maxHp: 150,
        attack: 25,
        defense: 10,
        speed: 1.2,
        status: 'ACTIVE' as const,
        diedAt: null,
        reviveAt: null,
        pvpWins: 0,
        pvpLosses: 0,
        deathCount: 0,
        createdAt: new Date(),
        updatedAt: new Date()
      }

      const powerScore = thrallService.calculatePowerScore(thrall)

      strictEqual(powerScore, 150 + 25 * 2 + 10 * 1.5)
    })
  })

  describe('syncProgression', () => {
    it('updates level and xp', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'sync-owner',
        archetype: '[WEREWOLF]'
      })

      const updated = await thrallService.syncProgression({
        thrallId: thrall.id,
        level: 5,
        xp: 1500
      })

      ok(updated)
      strictEqual(updated.level, 5)
      strictEqual(updated.xp, 1500)
    })

    it('updates optional stats when provided', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'sync-stats-owner',
        archetype: '[WEREWOLF]'
      })

      const updated = await thrallService.syncProgression({
        thrallId: thrall.id,
        level: 10,
        xp: 5000,
        attack: 50,
        defense: 25,
        maxHp: 300
      })

      ok(updated)
      strictEqual(updated.attack, 50)
      strictEqual(updated.defense, 25)
      strictEqual(updated.maxHp, 300)
    })

    it('returns null for non-existent thrall', async () => {
      const result = await thrallService.syncProgression({
        thrallId: 'fake-id',
        level: 5,
        xp: 1000
      })

      strictEqual(result, null)
    })
  })
})


