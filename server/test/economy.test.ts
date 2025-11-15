import { strictEqual } from 'node:assert/strict'
import test from 'node:test'

import { env } from '../src/config/env'
import { economyService } from '../src/modules/economy'
import { appRouter } from '../src/trpc/router'

void test('economy wallet sync applies deltas deterministically', async (t) => {
  const caller = appRouter.createCaller({
    requestId: 'test-economy',
    env
  })

  await t.test('increments balances', async () => {
    const result = await caller.economy.syncWallet({
      playerId: 'unit-player',
      duskenCoinDelta: 50,
      bloodShardDelta: 2
    })
    strictEqual(result.duskenCoinBalance, 1250)
    strictEqual(result.bloodShardBalance, 14)
  })

  await t.test('prevents negative balances', async () => {
    const result = await caller.economy.syncWallet({
      playerId: 'unit-player',
      duskenCoinDelta: -5000,
      bloodShardDelta: -50
    })
    strictEqual(result.duskenCoinBalance, 0)
    strictEqual(result.bloodShardBalance, 0)
    const wallet = await economyService.getWallet('unit-player')
    strictEqual(wallet.duskenCoinBalance, 0)
    strictEqual(wallet.bloodShardBalance, 0)
  })
})


