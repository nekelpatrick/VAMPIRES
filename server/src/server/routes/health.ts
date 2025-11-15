import { FastifyPluginAsync } from 'fastify'

import { createRequestId } from '../../lib/id'

export const registerHealthRoutes: FastifyPluginAsync = (fastify) => {
  fastify.get('/healthz', () => {
    return {
      id: createRequestId(),
      status: 'ok',
      nodeEnv: fastify.env.NODE_ENV
    }
  })

  fastify.get('/readyz', () => {
    return {
      status: 'ready',
      uptime: process.uptime()
    }
  })

  return Promise.resolve()
}

