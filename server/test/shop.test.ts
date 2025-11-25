import { strictEqual, ok } from 'node:assert/strict'
import test from 'node:test'

import { env } from '../src/config/env'
import { economyService } from '../src/modules/economy'
import { shopService } from '../src/modules/shop'
import { appRouter } from '../src/trpc/router'

void test('shop catalog returns available items', async (t) => {
  await t.test('getCatalog returns all available items', () => {
    const items = shopService.getCatalog()
    ok(items.length > 0)
    ok(items.every(item => item.available === true))
  })

  await t.test('getItem returns correct item by id', () => {
    const item = shopService.getItem('revive_token_1')
    ok(item !== undefined)
    strictEqual(item?.name, 'Revive Token')
    strictEqual(item?.price.duskenCoin, 500)
    strictEqual(item?.price.bloodShards, 0)
  })

  await t.test('getItem returns undefined for invalid id', () => {
    const item = shopService.getItem('invalid_item_id')
    strictEqual(item, undefined)
  })
})

void test('shop purchase validates balance', async (t) => {
  await t.test('purchase fails with insufficient dusken coin', async () => {
    const result = await shopService.purchase({
      playerId: 'poor-player-dusken',
      itemId: 'stat_boost_attack'
    })

    strictEqual(result.success, false)
    strictEqual(result.error, 'Insufficient [DUSKEN COIN]')
  })

  await t.test('purchase succeeds with sufficient balance', async () => {
    await economyService.addDuskenCoin('rich-player', 5000, 'admin_adjustment')

    const result = await shopService.purchase({
      playerId: 'rich-player',
      itemId: 'revive_token_1'
    })

    strictEqual(result.success, true)
    strictEqual(result.itemId, 'revive_token_1')
    ok(result.walletAfter !== undefined)
  })
})

void test('shop tRPC router', async (t) => {
  const caller = appRouter.createCaller({
    requestId: 'test-shop-trpc',
    env
  })

  await t.test('catalog returns array of shop items', async () => {
    const catalog = await caller.shop.catalog()
    ok(Array.isArray(catalog))
    ok(catalog.length > 0)
    ok(catalog[0].id !== undefined)
    ok(catalog[0].name !== undefined)
    ok(catalog[0].price !== undefined)
  })

  await t.test('item returns single item or null', async () => {
    const item = await caller.shop.item({ itemId: 'xp_booster_30m' })
    ok(item !== null)
    strictEqual(item?.id, 'xp_booster_30m')
  })

  await t.test('xpMultiplier returns multiplier for player', async () => {
    const result = await caller.shop.xpMultiplier({ playerId: 'test-xp-player' })
    ok(result.multiplier !== undefined)
    strictEqual(result.multiplier, 1)
  })
})

void test('shop item effects', async (t) => {
  await t.test('dusken pack adds currency', async () => {
    const walletBefore = await economyService.getWallet('dusken-pack-player')
    const initialDusken = walletBefore.duskenCoinBalance

    await economyService.addBloodShards('dusken-pack-player', 10, 'admin_adjustment')

    const result = await shopService.purchase({
      playerId: 'dusken-pack-player',
      itemId: 'dusken_pack_small'
    })

    strictEqual(result.success, true)
    ok(result.walletAfter !== undefined)
    strictEqual(result.walletAfter!.duskenCoinBalance, initialDusken + 1000)
  })

  await t.test('xp booster activates multiplier', async () => {
    await economyService.addDuskenCoin('xp-booster-player', 500, 'admin_adjustment')

    const result = await shopService.purchase({
      playerId: 'xp-booster-player',
      itemId: 'xp_booster_30m'
    })

    strictEqual(result.success, true)

    const multiplier = shopService.getXpMultiplier('xp-booster-player')
    strictEqual(multiplier, 2)
  })
})

