import { FastifyPluginAsync } from 'fastify'

import { createRequestId } from '../../lib/id'

export const registerHealthRoutes: FastifyPluginAsync = (fastify) => {
  fastify.get(
    '/healthz',
    {
      schema: {
        tags: ['Diagnostics'],
        summary: 'Liveness probe',
        response: {
          200: {
            type: 'object',
            properties: {
              id: { type: 'string' },
              status: { type: 'string' },
              nodeEnv: { type: 'string' }
            }
          }
        }
      }
    },
    () => {
      return {
        id: createRequestId(),
        status: 'ok',
        nodeEnv: fastify.env.NODE_ENV
      }
    }
  )

  fastify.get(
    '/readyz',
    {
      schema: {
        tags: ['Diagnostics'],
        summary: 'Readiness probe',
        response: {
          200: {
            type: 'object',
            properties: {
              status: { type: 'string' },
              uptime: { type: 'number' }
            }
          }
        }
      }
    },
    () => {
      return {
        status: 'ready',
        uptime: process.uptime()
      }
    }
  )

  return Promise.resolve()
}

