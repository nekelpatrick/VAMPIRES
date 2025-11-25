import type { FastifyPluginAsync, FastifyReply, FastifyRequest } from 'fastify'

import { questService } from '../../modules/quests'

export const questRoutes: FastifyPluginAsync = async (fastify) => {
  fastify.get(
    '/api/quests/daily',
    {
      schema: {
        tags: ['quests'],
        summary: 'Get daily quests for player',
        querystring: {
          type: 'object',
          required: ['playerId'],
          properties: {
            playerId: { type: 'string' }
          }
        },
        response: {
          200: {
            type: 'array',
            items: {
              type: 'object',
              properties: {
                id: { type: 'string' },
                questType: { type: 'string' },
                questName: { type: 'string' },
                description: { type: 'string' },
                targetValue: { type: 'number' },
                currentValue: { type: 'number' },
                duskenReward: { type: 'number' },
                bloodShardsReward: { type: 'number' },
                completed: { type: 'boolean' },
                claimed: { type: 'boolean' },
                adBonusClaimed: { type: 'boolean' }
              }
            }
          }
        }
      }
    },
    async (request: FastifyRequest<{ Querystring: { playerId: string } }>, reply: FastifyReply) => {
      const { playerId } = request.query

      if (!playerId) {
        return reply.status(400).send({ error: 'playerId is required' })
      }

      const quests = await questService.getDailyQuests(playerId)
      return reply.send(quests)
    }
  )

  fastify.post(
    '/api/quests/progress',
    {
      schema: {
        tags: ['quests'],
        summary: 'Update quest progress',
        body: {
          type: 'object',
          required: ['playerId', 'questType', 'value'],
          properties: {
            playerId: { type: 'string' },
            questType: { type: 'string' },
            value: { type: 'number' }
          }
        },
        response: {
          200: {
            type: 'object',
            properties: {
              updated: { type: 'boolean' },
              quest: { type: 'object' }
            }
          }
        }
      }
    },
    async (
      request: FastifyRequest<{
        Body: { playerId: string; questType: string; value: number }
      }>,
      reply: FastifyReply
    ) => {
      const { playerId, questType, value } = request.body

      const quest = await questService.updateProgress(
        playerId,
        questType as 'KillEnemies' | 'ReachWave' | 'DealDamage' | 'WinPvP' | 'GetCombo',
        value
      )

      return reply.send({ updated: !!quest, quest })
    }
  )

  fastify.post(
    '/api/quests/claim',
    {
      schema: {
        tags: ['quests'],
        summary: 'Claim quest reward',
        body: {
          type: 'object',
          required: ['playerId', 'questId'],
          properties: {
            playerId: { type: 'string' },
            questId: { type: 'string' },
            watchedAd: { type: 'boolean' }
          }
        },
        response: {
          200: {
            type: 'object',
            properties: {
              success: { type: 'boolean' },
              questId: { type: 'string' },
              duskenAwarded: { type: 'number' },
              bloodShardsAwarded: { type: 'number' },
              adBonusApplied: { type: 'boolean' }
            }
          },
          400: {
            type: 'object',
            properties: {
              error: { type: 'string' }
            }
          }
        }
      }
    },
    async (
      request: FastifyRequest<{
        Body: { playerId: string; questId: string; watchedAd?: boolean }
      }>,
      reply: FastifyReply
    ) => {
      const { playerId, questId, watchedAd = false } = request.body

      const result = await questService.claimQuest(playerId, questId, watchedAd)

      if (!result) {
        return reply.status(400).send({ error: 'Quest not found, not completed, or already claimed' })
      }

      return reply.send(result)
    }
  )
}

