import { strictEqual, ok } from 'node:assert/strict'
import test from 'node:test'

import { env } from '../src/config/env'
import { economyService } from '../src/modules/economy'
import { subscriptionService, ASHEN_ONE_BENEFITS, DEFAULT_BENEFITS } from '../src/modules/subscription'
import { appRouter } from '../src/trpc/router'

void test('subscription status', async (t) => {
  await t.test('non-premium player has default benefits', async () => {
    const status = await subscriptionService.getSubscriptionStatus('free-player')
    strictEqual(status.tier, 'NONE')
    strictEqual(status.active, false)
    strictEqual(status.benefits.offlineBonus, DEFAULT_BENEFITS.offlineBonus)
    strictEqual(status.benefits.reviveDiscount, DEFAULT_BENEFITS.reviveDiscount)
  })

  await t.test('getBenefits returns correct benefits for non-premium', async () => {
    const benefits = await subscriptionService.getBenefits('free-player-2')
    strictEqual(benefits.offlineBonus, 1.0)
    strictEqual(benefits.lootBonus, 1.0)
    strictEqual(benefits.pvpPriority, false)
  })
})

void test('subscription activation', async (t) => {
  await t.test('activateSubscription sets premium status', async () => {
    const status = await subscriptionService.activateSubscription({
      playerId: 'new-premium-player',
      durationDays: 30
    })

    strictEqual(status.tier, '[ASHEN ONE]')
    strictEqual(status.active, true)
    ok(status.expiresAt !== null)
    strictEqual(status.benefits.offlineBonus, ASHEN_ONE_BENEFITS.offlineBonus)
    strictEqual(status.benefits.reviveDiscount, ASHEN_ONE_BENEFITS.reviveDiscount)
    strictEqual(status.benefits.lootBonus, ASHEN_ONE_BENEFITS.lootBonus)
    strictEqual(status.benefits.pvpPriority, true)
  })

  await t.test('activation grants monthly blood shards', async () => {
    const walletBefore = await economyService.getWallet('shard-bonus-player')
    const initialShards = walletBefore.bloodShardBalance

    await subscriptionService.activateSubscription({
      playerId: 'shard-bonus-player',
      durationDays: 30
    })

    const walletAfter = await economyService.getWallet('shard-bonus-player')
    strictEqual(walletAfter.bloodShardBalance, initialShards + ASHEN_ONE_BENEFITS.monthlyBloodShards)
  })

  await t.test('isSubscribed returns true for active subscription', async () => {
    await subscriptionService.activateSubscription({
      playerId: 'check-sub-player',
      durationDays: 30
    })

    const isSubscribed = await subscriptionService.isSubscribed('check-sub-player')
    strictEqual(isSubscribed, true)
  })
})

void test('subscription cancellation', async (t) => {
  await t.test('cancelSubscription removes premium status', async () => {
    await subscriptionService.activateSubscription({
      playerId: 'cancel-player',
      durationDays: 30
    })

    const statusBefore = await subscriptionService.getSubscriptionStatus('cancel-player')
    strictEqual(statusBefore.active, true)

    const statusAfter = await subscriptionService.cancelSubscription('cancel-player')
    strictEqual(statusAfter.tier, 'NONE')
    strictEqual(statusAfter.active, false)
  })
})

void test('subscription tRPC router', async (t) => {
  const caller = appRouter.createCaller({
    requestId: 'test-subscription-trpc',
    env
  })

  await t.test('status returns subscription status', async () => {
    const status = await caller.subscription.status({ playerId: 'trpc-status-player' })
    ok(status.playerId === 'trpc-status-player')
    ok(status.tier !== undefined)
    ok(status.benefits !== undefined)
  })

  await t.test('benefits returns benefits object', async () => {
    const benefits = await caller.subscription.benefits({ playerId: 'trpc-benefits-player' })
    ok(typeof benefits.offlineBonus === 'number')
    ok(typeof benefits.reviveDiscount === 'number')
    ok(typeof benefits.lootBonus === 'number')
  })

  await t.test('activate creates subscription', async () => {
    const status = await caller.subscription.activate({
      playerId: 'trpc-activate-player',
      durationDays: 7
    })

    strictEqual(status.tier, '[ASHEN ONE]')
    strictEqual(status.active, true)
  })

  await t.test('cancel removes subscription', async () => {
    await caller.subscription.activate({
      playerId: 'trpc-cancel-player',
      durationDays: 7
    })

    const status = await caller.subscription.cancel({ playerId: 'trpc-cancel-player' })
    strictEqual(status.tier, 'NONE')
    strictEqual(status.active, false)
  })
})

void test('subscription benefit helpers', async (t) => {
  await t.test('getReviveDiscount returns correct value', () => {
    strictEqual(subscriptionService.getReviveDiscount(DEFAULT_BENEFITS), 0)
    strictEqual(subscriptionService.getReviveDiscount(ASHEN_ONE_BENEFITS), 0.5)
  })

  await t.test('getLootMultiplier returns correct value', () => {
    strictEqual(subscriptionService.getLootMultiplier(DEFAULT_BENEFITS), 1.0)
    strictEqual(subscriptionService.getLootMultiplier(ASHEN_ONE_BENEFITS), 1.25)
  })

  await t.test('getOfflineMultiplier returns correct value', () => {
    strictEqual(subscriptionService.getOfflineMultiplier(DEFAULT_BENEFITS), 1.0)
    strictEqual(subscriptionService.getOfflineMultiplier(ASHEN_ONE_BENEFITS), 1.5)
  })
})

