import assert from 'node:assert'
import { describe, it, before, after } from 'node:test'

import Fastify, { FastifyInstance } from 'fastify'

import app from '../src/app'

describe('Battle API', () => {
  let fastify: FastifyInstance

  before(async () => {
    fastify = Fastify()
    await fastify.register(app)
    await fastify.ready()
  })

  after(async () => {
    await fastify.close()
  })

  describe('POST /api/battle/start', () => {
    it('should start a battle and return result', async () => {
      const response = await fastify.inject({
        method: 'POST',
        url: '/api/battle/start',
        payload: {
          playerId: 'test-player-1',
          thrallId: 'test-thrall-1',
          wave: 1
        }
      })

      assert.strictEqual(response.statusCode, 200)

      const body = JSON.parse(response.body)
      assert.ok(body.battleId)
      assert.ok(body.result)
      assert.ok(['player', 'enemy', 'draw'].includes(body.result.winner))
      assert.ok(body.events)
      assert.ok(Array.isArray(body.events))
      assert.ok(body.rewards)
      assert.ok(typeof body.rewards.duskenCoin === 'number')
      assert.ok(typeof body.rewards.bloodShards === 'number')
      assert.ok(typeof body.rewards.xp === 'number')
    })

    it('should return 400 for invalid request', async () => {
      const response = await fastify.inject({
        method: 'POST',
        url: '/api/battle/start',
        payload: {
          playerId: '',
          thrallId: 'test-thrall-1',
          wave: 0
        }
      })

      assert.strictEqual(response.statusCode, 400)
    })

    it('should increase rewards for higher waves', async () => {
      const wave1Response = await fastify.inject({
        method: 'POST',
        url: '/api/battle/start',
        payload: {
          playerId: 'test-player-1',
          thrallId: 'test-thrall-1',
          wave: 1
        }
      })

      const wave10Response = await fastify.inject({
        method: 'POST',
        url: '/api/battle/start',
        payload: {
          playerId: 'test-player-1',
          thrallId: 'test-thrall-1',
          wave: 10
        }
      })

      const wave1Body = JSON.parse(wave1Response.body)
      const wave10Body = JSON.parse(wave10Response.body)

      assert.ok(wave10Body.rewards.xp >= wave1Body.rewards.xp)
    })
  })

  describe('GET /api/battle/:battleId', () => {
    it('should retrieve a battle by ID', async () => {
      const startResponse = await fastify.inject({
        method: 'POST',
        url: '/api/battle/start',
        payload: {
          playerId: 'test-player-2',
          thrallId: 'test-thrall-2',
          wave: 1
        }
      })

      const startBody = JSON.parse(startResponse.body)

      const getResponse = await fastify.inject({
        method: 'GET',
        url: `/api/battle/${startBody.battleId}`
      })

      assert.strictEqual(getResponse.statusCode, 200)

      const getBody = JSON.parse(getResponse.body)
      assert.strictEqual(getBody.id, startBody.battleId)
      assert.strictEqual(getBody.playerId, 'test-player-2')
      assert.strictEqual(getBody.wave, 1)
    })

    it('should return 404 for non-existent battle', async () => {
      const response = await fastify.inject({
        method: 'GET',
        url: '/api/battle/00000000-0000-0000-0000-000000000000'
      })

      assert.strictEqual(response.statusCode, 404)
    })
  })

  describe('GET /api/battle/player/:playerId', () => {
    it('should retrieve battles by player ID', async () => {
      const playerId = 'test-player-list'

      await fastify.inject({
        method: 'POST',
        url: '/api/battle/start',
        payload: {
          playerId,
          thrallId: 'test-thrall',
          wave: 1
        }
      })

      await fastify.inject({
        method: 'POST',
        url: '/api/battle/start',
        payload: {
          playerId,
          thrallId: 'test-thrall',
          wave: 2
        }
      })

      const response = await fastify.inject({
        method: 'GET',
        url: `/api/battle/player/${playerId}`
      })

      assert.strictEqual(response.statusCode, 200)

      const body = JSON.parse(response.body)
      assert.ok(Array.isArray(body))
      assert.ok(body.length >= 2)
      assert.ok(body.every((b: { playerId: string }) => b.playerId === playerId))
    })

    it('should support pagination', async () => {
      const playerId = 'test-player-pagination'

      for (let i = 0; i < 5; i++) {
        await fastify.inject({
          method: 'POST',
          url: '/api/battle/start',
          payload: {
            playerId,
            thrallId: 'test-thrall',
            wave: i + 1
          }
        })
      }

      const response = await fastify.inject({
        method: 'GET',
        url: `/api/battle/player/${playerId}?limit=2&offset=0`
      })

      assert.strictEqual(response.statusCode, 200)

      const body = JSON.parse(response.body)
      assert.strictEqual(body.length, 2)
    })
  })

  describe('GET /api/battle/:battleId/replay', () => {
    it('should replay a battle and return identical result', async () => {
      const startResponse = await fastify.inject({
        method: 'POST',
        url: '/api/battle/start',
        payload: {
          playerId: 'test-player-replay',
          thrallId: 'test-thrall-replay',
          wave: 1
        }
      })

      const startBody = JSON.parse(startResponse.body)

      const replayResponse = await fastify.inject({
        method: 'GET',
        url: `/api/battle/${startBody.battleId}/replay`
      })

      assert.strictEqual(replayResponse.statusCode, 200)

      const replayBody = JSON.parse(replayResponse.body)

      assert.strictEqual(replayBody.result.winner, startBody.result.winner)
      assert.strictEqual(replayBody.result.totalTicks, startBody.result.totalTicks)
      assert.strictEqual(replayBody.result.damageDealt, startBody.result.damageDealt)
    })
  })
})

