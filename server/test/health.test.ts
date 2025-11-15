import { strictEqual } from 'node:assert/strict'
import test from 'node:test'
import Fastify from 'fastify'

import app from '../src/app'

void test('health endpoints', async (t) => {
  const server = Fastify()
  await server.register(app)
  await server.ready()

  t.after(() => server.close())

  await t.test('/healthz responds', async () => {
    const response = await server.inject({ method: 'GET', url: '/healthz' })
    strictEqual(response.statusCode, 200)
  })

  await t.test('/trpc/health.ping responds', async () => {
    const response = await server.inject({ method: 'GET', url: '/trpc/health.ping' })
    strictEqual(response.statusCode, 200)
  })
})

