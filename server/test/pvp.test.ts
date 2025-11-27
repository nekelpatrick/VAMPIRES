import assert from 'node:assert'
import { describe, it } from 'node:test'

import {
  calculatePowerScore,
  getThreatLevel,
  isWithinMatchRange,
  findMatch,
  calculatePvpRewards,
  isQueueExpired,
  generateBotOpponent,
  calculateLoserPenalty
} from '../src/modules/pvp/matchmaker'
import type { Thrall } from '../src/modules/thrall/thrall.schema'
import type { QueueEntry } from '../src/modules/pvp/pvp.schema'

describe('Matchmaker', () => {
  describe('calculatePowerScore', () => {
    it('calculates power score correctly', () => {
      const thrall: Thrall = {
        id: 'test-1',
        ownerId: 'player-1',
        archetype: '[WEREWOLF]',
        level: 1,
        hp: 150,
        maxHp: 150,
        attack: 25,
        defense: 10,
        speed: 1.2,
        status: 'ACTIVE',
        diedAt: null,
        reviveAt: null,
        pvpWins: 0,
        pvpLosses: 0,
        deathCount: 0
      }

      const powerScore = calculatePowerScore(thrall)
      assert.strictEqual(powerScore, 150 + 25 * 2 + 10 * 1.5)
    })
  })

  describe('getThreatLevel', () => {
    it('returns LOW when opponent is much weaker', () => {
      assert.strictEqual(getThreatLevel(200, 100), 'LOW')
    })

    it('returns MODERATE when opponents are similar', () => {
      assert.strictEqual(getThreatLevel(200, 200), 'MODERATE')
      assert.strictEqual(getThreatLevel(200, 180), 'MODERATE')
      assert.strictEqual(getThreatLevel(200, 220), 'MODERATE')
    })

    it('returns HIGH when opponent is stronger', () => {
      assert.strictEqual(getThreatLevel(200, 300), 'HIGH')
    })
  })

  describe('isWithinMatchRange', () => {
    it('returns true for similar power scores', () => {
      assert.strictEqual(isWithinMatchRange(200, 200), true)
      assert.strictEqual(isWithinMatchRange(200, 210), true)
      assert.strictEqual(isWithinMatchRange(200, 240), true)
    })

    it('returns false for very different power scores', () => {
      assert.strictEqual(isWithinMatchRange(200, 300), false)
      assert.strictEqual(isWithinMatchRange(100, 200), false)
    })
  })

  describe('findMatch', () => {
    it('finds a match within range', () => {
      const entry: QueueEntry = {
        playerId: 'player-1',
        thrallId: 'thrall-1',
        powerScore: 200,
        joinedAt: new Date(),
        status: 'WAITING'
      }

      const queue: QueueEntry[] = [
        {
          playerId: 'player-2',
          thrallId: 'thrall-2',
          powerScore: 210,
          joinedAt: new Date(),
          status: 'WAITING'
        },
        {
          playerId: 'player-3',
          thrallId: 'thrall-3',
          powerScore: 500,
          joinedAt: new Date(),
          status: 'WAITING'
        }
      ]

      const match = findMatch(entry, queue)
      assert.ok(match)
      assert.strictEqual(match.playerId, 'player-2')
    })

    it('returns null when no match in range', () => {
      const entry: QueueEntry = {
        playerId: 'player-1',
        thrallId: 'thrall-1',
        powerScore: 200,
        joinedAt: new Date(),
        status: 'WAITING'
      }

      const queue: QueueEntry[] = [
        {
          playerId: 'player-2',
          thrallId: 'thrall-2',
          powerScore: 500,
          joinedAt: new Date(),
          status: 'WAITING'
        }
      ]

      const match = findMatch(entry, queue)
      assert.strictEqual(match, null)
    })

    it('does not match with self', () => {
      const entry: QueueEntry = {
        playerId: 'player-1',
        thrallId: 'thrall-1',
        powerScore: 200,
        joinedAt: new Date(),
        status: 'WAITING'
      }

      const match = findMatch(entry, [entry])
      assert.strictEqual(match, null)
    })

    it('does not match with non-WAITING entries', () => {
      const entry: QueueEntry = {
        playerId: 'player-1',
        thrallId: 'thrall-1',
        powerScore: 200,
        joinedAt: new Date(),
        status: 'WAITING'
      }

      const queue: QueueEntry[] = [
        {
          playerId: 'player-2',
          thrallId: 'thrall-2',
          powerScore: 210,
          joinedAt: new Date(),
          status: 'MATCHED'
        }
      ]

      const match = findMatch(entry, queue)
      assert.strictEqual(match, null)
    })
  })

  describe('calculatePvpRewards', () => {
    it('calculates rewards correctly', () => {
      const rewards = calculatePvpRewards(5, 5, false)
      assert.ok(rewards.duskenCoin > 0)
      assert.ok(rewards.rankingPoints > 0)
    })

    it('gives bonus for beating higher level opponent', () => {
      const normalRewards = calculatePvpRewards(5, 5, false)
      const bonusRewards = calculatePvpRewards(5, 10, false)
      assert.ok(bonusRewards.duskenCoin > normalRewards.duskenCoin)
    })

    it('halves rewards for bot matches', () => {
      const originalRandom = Math.random
      Math.random = () => 0.5

      const playerRewards = calculatePvpRewards(5, 5, false)
      const botRewards = calculatePvpRewards(5, 5, true)
      assert.strictEqual(botRewards.duskenCoin, Math.floor(playerRewards.duskenCoin * 0.5))

      Math.random = originalRandom
    })
  })

  describe('isQueueExpired', () => {
    it('returns false for recent entries', () => {
      const entry: QueueEntry = {
        playerId: 'player-1',
        thrallId: 'thrall-1',
        powerScore: 200,
        joinedAt: new Date(),
        status: 'WAITING'
      }

      assert.strictEqual(isQueueExpired(entry), false)
    })

    it('returns true for old entries', () => {
      const entry: QueueEntry = {
        playerId: 'player-1',
        thrallId: 'thrall-1',
        powerScore: 200,
        joinedAt: new Date(Date.now() - 60000),
        status: 'WAITING'
      }

      assert.strictEqual(isQueueExpired(entry), true)
    })

    it('returns true at exactly 30 seconds', () => {
      const entry: QueueEntry = {
        playerId: 'player-1',
        thrallId: 'thrall-1',
        powerScore: 200,
        joinedAt: new Date(Date.now() - 30000),
        status: 'WAITING'
      }

      assert.strictEqual(isQueueExpired(entry), true)
    })
  })

  describe('generateBotOpponent', () => {
    it('generates a bot with werewolf archetype', () => {
      const bot = generateBotOpponent(200)

      assert.ok(bot.thrallId.startsWith('bot-'))
      assert.strictEqual(bot.archetype, '[WEREWOLF]')
    })

    it('generates power score within variance range', () => {
      const playerPowerScore = 200
      const results: number[] = []

      for (let i = 0; i < 100; i++) {
        const bot = generateBotOpponent(playerPowerScore)
        results.push(bot.powerScore)
      }

      const min = Math.min(...results)
      const max = Math.max(...results)

      assert.ok(min >= playerPowerScore * 0.85, `Min ${min} should be >= ${playerPowerScore * 0.85}`)
      assert.ok(max <= playerPowerScore * 1.15, `Max ${max} should be <= ${playerPowerScore * 1.15}`)
    })
  })

  describe('calculateLoserPenalty', () => {
    it('returns negative ranking points', () => {
      const penalty = calculateLoserPenalty(5)

      assert.ok(penalty.rankingPoints < 0)
    })

    it('calculates penalty based on level', () => {
      const lowLevelPenalty = calculateLoserPenalty(1)
      const highLevelPenalty = calculateLoserPenalty(20)

      assert.strictEqual(lowLevelPenalty.rankingPoints, -5)
      assert.strictEqual(highLevelPenalty.rankingPoints, -20)
    })

    it('has minimum penalty of 5', () => {
      const penalty = calculateLoserPenalty(3)

      assert.strictEqual(penalty.rankingPoints, -5)
    })
  })
})
