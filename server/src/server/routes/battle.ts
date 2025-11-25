import type { FastifyInstance } from 'fastify'

import { battleService } from '../../modules/battle/battle.service'
import {
  startBattleRequestSchema,
  getBattleRequestSchema,
  getBattlesByPlayerRequestSchema
} from '../../modules/battle/battle.schema'

export async function battleRoutes(fastify: FastifyInstance): Promise<void> {
  fastify.post('/api/battle/start', {
    schema: {
      description: 'Start a new PvE battle',
      tags: ['battle'],
      body: {
        type: 'object',
        required: ['playerId', 'thrallId', 'wave'],
        properties: {
          playerId: { type: 'string', minLength: 1 },
          thrallId: { type: 'string', minLength: 1 },
          wave: { type: 'integer', minimum: 1 }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            battleId: { type: 'string', format: 'uuid' },
            result: {
              type: 'object',
              properties: {
                winner: { type: 'string', enum: ['player', 'enemy', 'draw'] },
                totalTicks: { type: 'integer' },
                thrallSurvived: { type: 'boolean' },
                enemiesKilled: { type: 'integer' },
                damageDealt: { type: 'integer' },
                damageTaken: { type: 'integer' }
              }
            },
            events: { type: 'array' },
            rewards: {
              type: 'object',
              properties: {
                duskenCoin: { type: 'integer' },
                bloodShards: { type: 'integer' },
                xp: { type: 'integer' }
              }
            }
          }
        },
        400: {
          type: 'object',
          properties: {
            error: { type: 'string' },
            details: { type: 'object' }
          }
        }
      }
    },
    handler: async (request, reply) => {
      const parsed = startBattleRequestSchema.safeParse(request.body)

      if (!parsed.success) {
        return reply.status(400).send({
          error: 'Invalid request',
          details: parsed.error.flatten()
        })
      }

      const response = await battleService.startBattle(parsed.data)
      return reply.send(response)
    }
  })

  fastify.get('/api/battle/:battleId', {
    schema: {
      description: 'Get battle by ID',
      tags: ['battle'],
      params: {
        type: 'object',
        required: ['battleId'],
        properties: {
          battleId: { type: 'string', format: 'uuid' }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            id: { type: 'string', format: 'uuid' },
            playerId: { type: 'string' },
            thrallId: { type: 'string' },
            wave: { type: 'integer' },
            seed: { type: 'integer' },
            result: { type: 'object' },
            events: { type: 'array' },
            createdAt: { type: 'string', format: 'date-time' }
          }
        },
        400: {
          type: 'object',
          properties: {
            error: { type: 'string' },
            details: { type: 'object' }
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
      const { battleId } = request.params as { battleId: string }

      const parsed = getBattleRequestSchema.safeParse({ battleId })
      if (!parsed.success) {
        return reply.status(400).send({
          error: 'Invalid request',
          details: parsed.error.flatten()
        })
      }

      const battle = await battleService.getBattle(parsed.data.battleId)

      if (!battle) {
        return reply.status(404).send({ error: 'Battle not found' })
      }

      return reply.send(battle)
    }
  })

  fastify.get('/api/battle/player/:playerId', {
    schema: {
      description: 'Get battles by player ID',
      tags: ['battle'],
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
          limit: { type: 'integer', minimum: 1, maximum: 100, default: 20 },
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
              playerId: { type: 'string' },
              thrallId: { type: 'string' },
              wave: { type: 'integer' },
              seed: { type: 'integer' },
              result: { type: 'object' },
              events: { type: 'array' },
              createdAt: { type: 'string', format: 'date-time' }
            }
          }
        },
        400: {
          type: 'object',
          properties: {
            error: { type: 'string' },
            details: { type: 'object' }
          }
        }
      }
    },
    handler: async (request, reply) => {
      const { playerId } = request.params as { playerId: string }
      const { limit, offset } = request.query as { limit?: number; offset?: number }

      const parsed = getBattlesByPlayerRequestSchema.safeParse({
        playerId,
        limit: limit ?? 20,
        offset: offset ?? 0
      })

      if (!parsed.success) {
        return reply.status(400).send({
          error: 'Invalid request',
          details: parsed.error.flatten()
        })
      }

      const battles = await battleService.getBattlesByPlayer(
        parsed.data.playerId,
        parsed.data.limit,
        parsed.data.offset
      )

      return reply.send(battles)
    }
  })

  fastify.get('/api/battle/:battleId/replay', {
    schema: {
      description: 'Replay a battle (re-run deterministic simulation)',
      tags: ['battle'],
      params: {
        type: 'object',
        required: ['battleId'],
        properties: {
          battleId: { type: 'string', format: 'uuid' }
        }
      },
      response: {
        200: {
          type: 'object',
          properties: {
            result: {
              type: 'object',
              properties: {
                winner: { type: 'string', enum: ['player', 'enemy', 'draw'] },
                totalTicks: { type: 'integer' },
                thrallSurvived: { type: 'boolean' },
                enemiesKilled: { type: 'integer' },
                damageDealt: { type: 'integer' },
                damageTaken: { type: 'integer' }
              }
            },
            events: { type: 'array' },
            seed: { type: 'integer' }
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
      const { battleId } = request.params as { battleId: string }

      const outcome = await battleService.replayBattle(battleId)

      if (!outcome) {
        return reply.status(404).send({ error: 'Battle not found' })
      }

      return reply.send({
        result: outcome.result,
        events: outcome.events,
        seed: outcome.seed
      })
    }
  })
}

