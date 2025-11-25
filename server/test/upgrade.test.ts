import { strictEqual, ok } from 'node:assert/strict'
import test from 'node:test'

import { env } from '../src/config/env'
import { economyService } from '../src/modules/economy'
import { upgradeService, calculateUpgradeCost } from '../src/modules/economy'
import { thrallService } from '../src/modules/thrall'
import { appRouter } from '../src/trpc/router'

void test('upgrade cost calculation', async (t) => {
  await t.test('calculateUpgradeCost returns correct base costs at level 0', () => {
    const attackCost = calculateUpgradeCost('attack', 0)
    strictEqual(attackCost.duskenCost, 100)
    strictEqual(attackCost.bloodShardCost, 0)
    strictEqual(attackCost.statIncrease, 5)

    const defenseCost = calculateUpgradeCost('defense', 0)
    strictEqual(defenseCost.duskenCost, 80)
    strictEqual(defenseCost.bloodShardCost, 0)
    strictEqual(defenseCost.statIncrease, 3)

    const maxHpCost = calculateUpgradeCost('maxHp', 0)
    strictEqual(maxHpCost.duskenCost, 120)
    strictEqual(maxHpCost.bloodShardCost, 0)
    strictEqual(maxHpCost.statIncrease, 20)

    const speedCost = calculateUpgradeCost('speed', 0)
    strictEqual(speedCost.duskenCost, 150)
    strictEqual(speedCost.bloodShardCost, 1)
    strictEqual(speedCost.statIncrease, 0.1)
  })

  await t.test('calculateUpgradeCost scales with level', () => {
    const level0 = calculateUpgradeCost('attack', 0)
    const level4 = calculateUpgradeCost('attack', 4)

    ok(level4.duskenCost > level0.duskenCost)
    strictEqual(level4.duskenCost, Math.floor(100 * (1 + 4 * 0.25)))
  })
})

void test('upgrade service stat levels', async (t) => {
  await t.test('getStatLevel returns 0 for new thrall', () => {
    const level = upgradeService.getStatLevel('new-thrall-123', 'attack')
    strictEqual(level, 0)
  })

  await t.test('getUpgradeCost uses current stat level', () => {
    const cost = upgradeService.getUpgradeCost('level-test-thrall', 'defense')
    strictEqual(cost.currentLevel, 0)
    strictEqual(cost.duskenCost, 80)
  })

  await t.test('getAllUpgradeCosts returns all four stats', () => {
    const costs = upgradeService.getAllUpgradeCosts('all-costs-thrall')
    strictEqual(costs.length, 4)
    ok(costs.some(c => c.stat === 'attack'))
    ok(costs.some(c => c.stat === 'defense'))
    ok(costs.some(c => c.stat === 'maxHp'))
    ok(costs.some(c => c.stat === 'speed'))
  })
})

void test('upgrade thrall validation', async (t) => {
  await t.test('upgrade fails for non-existent thrall', async () => {
    const result = await upgradeService.upgradeThrall({
      playerId: 'player-no-thrall',
      thrallId: 'non-existent-thrall',
      stat: 'attack'
    })

    strictEqual(result.success, false)
    strictEqual(result.error, 'Thrall not found')
  })

  await t.test('upgrade fails for wrong owner', async () => {
    const thrall = await thrallService.createThrall({
      ownerId: 'owner-player',
      archetype: '[WEREWOLF]'
    })

    const result = await upgradeService.upgradeThrall({
      playerId: 'different-player',
      thrallId: thrall.id,
      stat: 'attack'
    })

    strictEqual(result.success, false)
    strictEqual(result.error, 'Thrall does not belong to player')
  })

  await t.test('upgrade fails with insufficient currency', async () => {
    const thrall = await thrallService.createThrall({
      ownerId: 'poor-upgrade-player',
      archetype: '[WEREWOLF]'
    })

    const wallet = await economyService.getWallet('poor-upgrade-player')
    if (wallet.duskenCoinBalance > 0) {
      await economyService.applyDelta({
        playerId: 'poor-upgrade-player',
        duskenCoinDelta: -wallet.duskenCoinBalance,
        bloodShardDelta: -wallet.bloodShardBalance
      })
    }

    const result = await upgradeService.upgradeThrall({
      playerId: 'poor-upgrade-player',
      thrallId: thrall.id,
      stat: 'attack'
    })

    strictEqual(result.success, false)
    strictEqual(result.error, 'Insufficient [DUSKEN COIN]')
  })
})

void test('upgrade thrall success', async (t) => {
  await t.test('upgrade succeeds with sufficient balance', async () => {
    const thrall = await thrallService.createThrall({
      ownerId: 'rich-upgrade-player',
      archetype: '[WEREWOLF]'
    })

    await economyService.addDuskenCoin('rich-upgrade-player', 1000, 'admin_adjustment')

    const result = await upgradeService.upgradeThrall({
      playerId: 'rich-upgrade-player',
      thrallId: thrall.id,
      stat: 'attack'
    })

    strictEqual(result.success, true)
    ok(result.newStatValue !== undefined)
    ok(result.newLevel !== undefined)
    strictEqual(result.newLevel, 1)
    ok(result.walletAfter !== undefined)
  })

  await t.test('upgrade increments stat level', async () => {
    const thrall = await thrallService.createThrall({
      ownerId: 'level-up-player',
      archetype: '[WEREWOLF]'
    })

    await economyService.addDuskenCoin('level-up-player', 500, 'admin_adjustment')

    const levelBefore = upgradeService.getStatLevel(thrall.id, 'defense')

    await upgradeService.upgradeThrall({
      playerId: 'level-up-player',
      thrallId: thrall.id,
      stat: 'defense'
    })

    const levelAfter = upgradeService.getStatLevel(thrall.id, 'defense')
    strictEqual(levelAfter, levelBefore + 1)
  })
})

void test('upgrade tRPC router', async (t) => {
  const caller = appRouter.createCaller({
    requestId: 'test-upgrade-trpc',
    env
  })

  await t.test('upgradeCost returns cost for stat', async () => {
    const thrall = await thrallService.createThrall({
      ownerId: 'trpc-cost-player',
      archetype: '[WEREWOLF]'
    })

    const cost = await caller.economy.upgradeCost({
      thrallId: thrall.id,
      stat: 'attack'
    })

    ok(cost.stat === 'attack')
    ok(typeof cost.duskenCost === 'number')
    ok(typeof cost.bloodShardCost === 'number')
    ok(typeof cost.statIncrease === 'number')
  })

  await t.test('allUpgradeCosts returns array of costs', async () => {
    const thrall = await thrallService.createThrall({
      ownerId: 'trpc-all-costs-player',
      archetype: '[WEREWOLF]'
    })

    const costs = await caller.economy.allUpgradeCosts({ thrallId: thrall.id })
    strictEqual(costs.length, 4)
  })

  await t.test('upgradeThrall processes upgrade', async () => {
    const thrall = await thrallService.createThrall({
      ownerId: 'trpc-upgrade-player',
      archetype: '[WEREWOLF]'
    })

    await economyService.addDuskenCoin('trpc-upgrade-player', 500, 'admin_adjustment')

    const result = await caller.economy.upgradeThrall({
      playerId: 'trpc-upgrade-player',
      thrallId: thrall.id,
      stat: 'maxHp'
    })

    strictEqual(result.success, true)
  })
})

