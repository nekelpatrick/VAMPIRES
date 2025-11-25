import assert from 'node:assert'
import { describe, it } from 'node:test'

import { revivalService } from '../src/modules/revival/revival.service'
import { thrallService } from '../src/modules/thrall/thrall.service'

describe('Revival System', () => {
  describe('getRevivalStatus', () => {
    it('returns null for non-existent thrall', async () => {
      const status = await revivalService.getRevivalStatus('non-existent-' + Date.now())
      assert.strictEqual(status, null)
    })

    it('returns status for active thrall', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'player-revival-test-1-' + Date.now(),
        archetype: '[WEREWOLF]'
      })

      const status = await revivalService.getRevivalStatus(thrall.id)
      assert.ok(status)
      assert.strictEqual(status.status, 'ACTIVE')
      assert.strictEqual(status.canRevive, false)
    })

    it('shows revival options for dead thrall', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'player-revival-test-2-' + Date.now(),
        archetype: '[WEREWOLF]'
      })

      await thrallService.updateStatus(thrall.id, 'DEAD', new Date(), null)

      const status = await revivalService.getRevivalStatus(thrall.id)
      assert.ok(status)
      assert.strictEqual(status.canRevive, true)
      assert.strictEqual(status.revivalOptions.length, 3)
      assert.strictEqual(status.revivalOptions[0].method, 'FREE')
      assert.strictEqual(status.revivalOptions[1].method, 'DUSKEN_COIN')
      assert.strictEqual(status.revivalOptions[2].method, 'BLOOD_SHARDS')
    })

    it('applies premium discounts', async () => {
      const thrall = await thrallService.createThrall({
        ownerId: 'player-revival-test-3-' + Date.now(),
        archetype: '[WEREWOLF]'
      })

      await thrallService.updateStatus(thrall.id, 'DEAD', new Date(), null)

      const normalStatus = await revivalService.getRevivalStatus(thrall.id, false)
      const premiumStatus = await revivalService.getRevivalStatus(thrall.id, true)

      const normalCost = normalStatus?.revivalOptions.find((o) => o.method === 'DUSKEN_COIN')?.cost ?? 0
      const premiumCost = premiumStatus?.revivalOptions.find((o) => o.method === 'DUSKEN_COIN')?.cost ?? 0

      assert.strictEqual(premiumCost, Math.floor(normalCost * 0.5))
    })
  })

  describe('startRevival', () => {
    it('fails for non-existent thrall', async () => {
      const result = await revivalService.startRevival({
        playerId: 'player-revival-test-4-' + Date.now(),
        thrallId: 'non-existent-' + Date.now(),
        method: 'FREE'
      })

      assert.strictEqual(result.success, false)
      assert.strictEqual(result.error, 'Thrall not found')
    })

    it('fails if thrall is not dead', async () => {
      const playerId = 'player-revival-test-5-' + Date.now()
      const thrall = await thrallService.createThrall({
        ownerId: playerId,
        archetype: '[WEREWOLF]'
      })

      const result = await revivalService.startRevival({
        playerId,
        thrallId: thrall.id,
        method: 'FREE'
      })

      assert.strictEqual(result.success, false)
      assert.strictEqual(result.error, 'Thrall is not dead')
    })

    it('fails if player does not own thrall', async () => {
      const playerId = 'player-revival-test-6-' + Date.now()
      const thrall = await thrallService.createThrall({
        ownerId: playerId,
        archetype: '[WEREWOLF]'
      })

      await thrallService.updateStatus(thrall.id, 'DEAD', new Date(), null)

      const result = await revivalService.startRevival({
        playerId: 'different-player-' + Date.now(),
        thrallId: thrall.id,
        method: 'FREE'
      })

      assert.strictEqual(result.success, false)
      assert.strictEqual(result.error, 'Thrall does not belong to player')
    })

    it('starts free revival with timer', async () => {
      const playerId = 'player-revival-test-7-' + Date.now()
      const thrall = await thrallService.createThrall({
        ownerId: playerId,
        archetype: '[WEREWOLF]'
      })

      await thrallService.updateStatus(thrall.id, 'DEAD', new Date(), null)

      const result = await revivalService.startRevival({
        playerId,
        thrallId: thrall.id,
        method: 'FREE'
      })

      assert.strictEqual(result.success, true)
      assert.strictEqual(result.immediate, false)
      assert.ok(result.reviveAt)

      const updatedThrall = await thrallService.getThrall(thrall.id)
      assert.strictEqual(updatedThrall?.status, 'REVIVING')
    })

    it('blood shards revival is instant', async () => {
      const playerId = 'player-revival-test-8-' + Date.now()
      const thrall = await thrallService.createThrall({
        ownerId: playerId,
        archetype: '[WEREWOLF]'
      })

      await thrallService.updateStatus(thrall.id, 'DEAD', new Date(), null)

      const result = await revivalService.startRevival({
        playerId,
        thrallId: thrall.id,
        method: 'BLOOD_SHARDS'
      })

      assert.strictEqual(result.success, true)
      assert.strictEqual(result.immediate, true)
      assert.strictEqual(result.reviveAt, null)

      const updatedThrall = await thrallService.getThrall(thrall.id)
      assert.strictEqual(updatedThrall?.status, 'ACTIVE')
    })
  })

  describe('completeRevival', () => {
    it('completes revival when timer expired', async () => {
      const playerId = 'player-revival-test-9-' + Date.now()
      const thrall = await thrallService.createThrall({
        ownerId: playerId,
        archetype: '[WEREWOLF]'
      })

      const expiredTime = new Date(Date.now() - 1000)
      await thrallService.updateStatus(thrall.id, 'REVIVING', new Date(), expiredTime)

      const completed = await revivalService.completeRevival(thrall.id)
      assert.strictEqual(completed, true)

      const updatedThrall = await thrallService.getThrall(thrall.id)
      assert.strictEqual(updatedThrall?.status, 'ACTIVE')
      assert.strictEqual(updatedThrall?.hp, updatedThrall?.maxHp)
    })

    it('does not complete revival if timer not expired', async () => {
      const playerId = 'player-revival-test-10-' + Date.now()
      const thrall = await thrallService.createThrall({
        ownerId: playerId,
        archetype: '[WEREWOLF]'
      })

      const futureTime = new Date(Date.now() + 60000)
      await thrallService.updateStatus(thrall.id, 'REVIVING', new Date(), futureTime)

      const completed = await revivalService.completeRevival(thrall.id)
      assert.strictEqual(completed, false)

      const updatedThrall = await thrallService.getThrall(thrall.id)
      assert.strictEqual(updatedThrall?.status, 'REVIVING')
    })
  })
})
