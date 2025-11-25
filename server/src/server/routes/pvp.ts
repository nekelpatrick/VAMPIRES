import type { FastifyInstance } from 'fastify'

import { pvpService } from '../../modules/pvp/pvp.service'
import {
  joinQueueRequestSchema,
  leaveQueueRequestSchema,
  pvpBattleRequestSchema,
  getPvpHistoryRequestSchema
} from '../../modules/pvp/pvp.schema'

export async function pvpRoutes(fastify: FastifyInstance): Promise<void> {
  fastify.post('/api/pvp/queue', {
    schema: {
      description: 'Join PvP matchmaking queue',
      tags: ['pvp'],
      body: {
        type: 'object',
        required: ['playerId', 'thrallId'],
        properties: {
          playerId: { type: 'string', minLength: 1 },
          thrallId: { type: 'string', minLength: 1 }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            success: { type: 'boolean' },
            matchFound: {
              type: 'object',
              nullable: true,
              properties: {
                matchId: { type: 'string', format: 'uuid' },
                opponentThreatLevel: { type: 'string', enum: ['LOW', 'MODERATE', 'HIGH', 'UNKNOWN'] },
                opponentArchetype: { type: 'string' }
              }
            }
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
      const parsed = joinQueueRequestSchema.safeParse(request.body)

      if (!parsed.success) {
        reply.code(400)
        return { error: 'Invalid request' }
      }

      const result = await pvpService.joinQueue(parsed.data.playerId, parsed.data.thrallId)

      if (!result.success) {
        reply.code(400)
        return { error: result.error }
      }

      return {
        success: true,
        matchFound: result.matchFound ?? null
      }
    }
  })

  fastify.delete('/api/pvp/queue', {
    schema: {
      description: 'Leave PvP matchmaking queue',
      tags: ['pvp'],
      body: {
        type: 'object',
        required: ['playerId'],
        properties: {
          playerId: { type: 'string', minLength: 1 }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            success: { type: 'boolean' }
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
      const parsed = leaveQueueRequestSchema.safeParse(request.body)

      if (!parsed.success) {
        reply.code(400)
        return { error: 'Invalid request' }
      }

      const success = await pvpService.leaveQueue(parsed.data.playerId)
      return { success }
    }
  })

  fastify.get('/api/pvp/queue/:playerId', {
    schema: {
      description: 'Get queue status for player',
      tags: ['pvp'],
      params: {
        type: 'object',
        required: ['playerId'],
        properties: {
          playerId: { type: 'string', minLength: 1 }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            inQueue: { type: 'boolean' },
            position: { type: 'integer' },
            waitTime: { type: 'integer' },
            estimatedWait: { type: 'integer' }
          }
        }
      }
    },
    handler: async (request) => {
      const { playerId } = request.params as { playerId: string }
      return pvpService.getQueueStatus(playerId)
    }
  })

  fastify.post('/api/pvp/battle/:matchId/resolve', {
    schema: {
      description: 'Resolve a PvP battle',
      tags: ['pvp'],
      params: {
        type: 'object',
        required: ['matchId'],
        properties: {
          matchId: { type: 'string', format: 'uuid' }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            matchId: { type: 'string', format: 'uuid' },
            battleId: { type: 'string', format: 'uuid' },
            winnerId: { type: 'string' },
            loserId: { type: 'string' },
            winnerRewards: {
              type: 'object',
              properties: {
                duskenCoin: { type: 'integer' },
                bloodShards: { type: 'integer' },
                rankingPoints: { type: 'integer' }
              }
            },
            loserPenalty: {
              type: 'object',
              properties: {
                thrallDied: { type: 'boolean' },
                rankingPoints: { type: 'integer' }
              }
            },
            events: { type: 'array' }
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
      const { matchId } = request.params as { matchId: string }
      const parsed = pvpBattleRequestSchema.safeParse({ matchId })

      if (!parsed.success) {
        reply.code(400)
        return { error: 'Invalid request' }
      }

      const result = await pvpService.resolvePvpBattle(parsed.data.matchId)

      if (!result) {
        reply.code(404)
        return { error: 'Match not found or already resolved' }
      }

      return result
    }
  })

  fastify.get('/api/pvp/match/:matchId', {
    schema: {
      description: 'Get PvP match details',
      tags: ['pvp'],
      params: {
        type: 'object',
        required: ['matchId'],
        properties: {
          matchId: { type: 'string', format: 'uuid' }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            id: { type: 'string', format: 'uuid' },
            player1Id: { type: 'string' },
            player1ThrallId: { type: 'string' },
            player2Id: { type: 'string' },
            player2ThrallId: { type: 'string' },
            winnerId: { type: 'string', nullable: true },
            loserId: { type: 'string', nullable: true },
            battleId: { type: 'string', format: 'uuid', nullable: true },
            status: { type: 'string' },
            createdAt: { type: 'string', format: 'date-time' }
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
      const { matchId } = request.params as { matchId: string }
      const match = await pvpService.getMatch(matchId)

      if (!match) {
        reply.code(404)
        return { error: 'Match not found' }
      }

      return match
    }
  })

  fastify.get('/api/pvp/history/:playerId', {
    schema: {
      description: 'Get PvP history for player',
      tags: ['pvp'],
      params: {
        type: 'object',
        required: ['playerId'],
        properties: {
          playerId: { type: 'string', minLength: 1 }
        }
      },
      querystring: {
        type: 'object',
        properties: {
          limit: { type: 'integer', minimum: 1, maximum: 50, default: 20 },
          offset: { type: 'integer', minimum: 0, default: 0 }
        }
      },
      response: {
        200: {
          type: 'array',
          items: {
            type: 'object',
            properties: {
              id: { type: 'string', format: 'uuid' },
              player1Id: { type: 'string' },
              player2Id: { type: 'string' },
              winnerId: { type: 'string', nullable: true },
              status: { type: 'string' },
              createdAt: { type: 'string', format: 'date-time' }
            }
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
      const { playerId } = request.params as { playerId: string }
      const { limit, offset } = request.query as { limit?: number; offset?: number }

      const parsed = getPvpHistoryRequestSchema.safeParse({
        playerId,
        limit: limit ?? 20,
        offset: offset ?? 0
      })

      if (!parsed.success) {
        reply.code(400)
        return { error: 'Invalid request' }
      }

      const history = await pvpService.getPvpHistory(
        parsed.data.playerId,
        parsed.data.limit,
        parsed.data.offset
      )

      return history
    }
  })
}

