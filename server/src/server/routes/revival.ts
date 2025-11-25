import type { FastifyInstance } from 'fastify'

import { revivalService } from '../../modules/revival/revival.service'
import {
  reviveRequestSchema,
  getRevivalStatusRequestSchema
} from '../../modules/revival/revival.schema'

export async function revivalRoutes(fastify: FastifyInstance): Promise<void> {
  fastify.get('/api/thrall/:thrallId/revival-status', {
    schema: {
      description: 'Get revival status for a thrall',
      tags: ['revival'],
      params: {
        type: 'object',
        required: ['thrallId'],
        properties: {
          thrallId: { type: 'string', minLength: 1 }
        }
      },
      querystring: {
        type: 'object',
        properties: {
          isPremium: { type: 'boolean', default: false }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            thrallId: { type: 'string' },
            status: { type: 'string', enum: ['DEAD', 'REVIVING', 'ACTIVE'] },
            diedAt: { type: 'string', format: 'date-time', nullable: true },
            reviveAt: { type: 'string', format: 'date-time', nullable: true },
            timeRemaining: { type: 'integer', nullable: true },
            canRevive: { type: 'boolean' },
            revivalOptions: {
              type: 'array',
              items: {
                type: 'object',
                properties: {
                  method: { type: 'string', enum: ['FREE', 'DUSKEN_COIN', 'BLOOD_SHARDS'] },
                  cost: { type: 'integer' },
                  currency: { type: 'string' },
                  timeSeconds: { type: 'integer' }
                }
              }
            }
          }
        },
        400: {
          type: 'object',
          properties: {
            error: { type: 'string' }
          }
        },
        404: {
          type: 'object',
          properties: {
            error: { type: 'string' }
          }
        }
      }
    },
    handler: async (request, reply) => {
      const { thrallId } = request.params as { thrallId: string }
      const { isPremium } = request.query as { isPremium?: boolean }

      const parsed = getRevivalStatusRequestSchema.safeParse({ thrallId })

      if (!parsed.success) {
        reply.code(400)
        return { error: 'Invalid request' }
      }

      const status = await revivalService.getRevivalStatus(
        parsed.data.thrallId,
        isPremium ?? false
      )

      if (!status) {
        reply.code(404)
        return { error: 'Thrall not found' }
      }

      return status
    }
  })

  fastify.post('/api/thrall/:thrallId/revive', {
    schema: {
      description: 'Start thrall revival',
      tags: ['revival'],
      params: {
        type: 'object',
        required: ['thrallId'],
        properties: {
          thrallId: { type: 'string', minLength: 1 }
        }
      },
      body: {
        type: 'object',
        required: ['playerId', 'method'],
        properties: {
          playerId: { type: 'string', minLength: 1 },
          method: { type: 'string', enum: ['FREE', 'DUSKEN_COIN', 'BLOOD_SHARDS'] }
        }
      },
      querystring: {
        type: 'object',
        properties: {
          isPremium: { type: 'boolean', default: false }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            success: { type: 'boolean' },
            thrallId: { type: 'string' },
            method: { type: 'string' },
            cost: { type: 'integer' },
            reviveAt: { type: 'string', format: 'date-time', nullable: true },
            immediate: { type: 'boolean' }
          }
        },
        400: {
          type: 'object',
          properties: {
            error: { type: 'string' }
          }
        }
      }
    },
    handler: async (request, reply) => {
      const { thrallId } = request.params as { thrallId: string }
      const { playerId, method } = request.body as { playerId: string; method: string }
      const { isPremium } = request.query as { isPremium?: boolean }

      const parsed = reviveRequestSchema.safeParse({ playerId, thrallId, method })

      if (!parsed.success) {
        reply.code(400)
        return { error: 'Invalid request' }
      }

      const result = await revivalService.startRevival(
        parsed.data,
        isPremium ?? false
      )

      if (!result.success) {
        reply.code(400)
        return { error: result.error }
      }

      return {
        success: result.success,
        thrallId: result.thrallId,
        method: result.method,
        cost: result.cost,
        reviveAt: result.reviveAt,
        immediate: result.immediate
      }
    }
  })

  fastify.post('/api/thrall/:thrallId/complete-revival', {
    schema: {
      description: 'Complete thrall revival (if timer expired)',
      tags: ['revival'],
      params: {
        type: 'object',
        required: ['thrallId'],
        properties: {
          thrallId: { type: 'string', minLength: 1 }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            success: { type: 'boolean' },
            thrallId: { type: 'string' }
          }
        }
      }
    },
    handler: async (request) => {
      const { thrallId } = request.params as { thrallId: string }
      const success = await revivalService.checkAndCompleteRevival(thrallId)
      return { success, thrallId }
    }
  })
}
