import { strictEqual, ok } from 'node:assert/strict'
import test from 'node:test'

import { env } from '../src/config/env'
import { appRouter } from '../src/trpc/router'

void test('player profile', async (t) => {
  const caller = appRouter.createCaller({
    requestId: 'test-player',
    env
  })

  await t.test('getProfile returns player data', async () => {
    const result = await caller.player.getProfile({ playerId: 'test-player-1' })

    strictEqual(result.id, 'test-player-1')
    ok(result.displayName.length > 0)
    ok(['CLAN NOCTURNUM', 'CLAN SABLEHEART', 'CLAN ECLIPSA'].includes(result.clan))
    ok(typeof result.duskenCoinBalance === 'number')
    ok(typeof result.bloodShardBalance === 'number')
    ok(['NONE', '[ASHEN ONE]'].includes(result.premiumStatus))
  })

  await t.test('getProfile uses provided playerId', async () => {
    const result = await caller.player.getProfile({ playerId: 'custom-id-123' })
    strictEqual(result.id, 'custom-id-123')
  })
})


