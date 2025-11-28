import { strictEqual, ok, notStrictEqual } from 'node:assert/strict'
import { describe, it, beforeEach } from 'node:test'

import { QuestService } from '../src/modules/quests/quest.service'
import type { QuestType } from '../src/modules/quests/quest.schema'

describe('QuestService', () => {
  let questService: QuestService

  beforeEach(() => {
    questService = new QuestService()
  })

  describe('getDailyQuests', () => {
    it('returns all daily quests for a player', async () => {
      const quests = await questService.getDailyQuests('test-player-1')

      ok(Array.isArray(quests))
      strictEqual(quests.length, 5)
      ok(quests.every((q) => q.playerId === 'test-player-1'))
    })

    it('returns cached quests on subsequent calls', async () => {
      const quests1 = await questService.getDailyQuests('test-player-2')
      const quests2 = await questService.getDailyQuests('test-player-2')

      strictEqual(quests1.length, quests2.length)
      strictEqual(quests1[0].id, quests2[0].id)
    })

    it('creates quests with correct initial values', async () => {
      const quests = await questService.getDailyQuests('test-player-3')

      ok(quests.every((q) => q.currentValue === 0))
      ok(quests.every((q) => q.completed === false))
      ok(quests.every((q) => q.claimed === false))
      ok(quests.every((q) => q.adBonusClaimed === false))
    })

    it('creates different quests for different players', async () => {
      const quests1 = await questService.getDailyQuests('player-a')
      const quests2 = await questService.getDailyQuests('player-b')

      notStrictEqual(quests1[0].id, quests2[0].id)
    })
  })

  describe('updateProgress', () => {
    it('updates quest progress', async () => {
      await questService.getDailyQuests('progress-player')

      const result = await questService.updateProgress(
        'progress-player',
        'KillEnemies' as QuestType,
        50
      )

      ok(result)
      strictEqual(result.currentValue, 50)
      strictEqual(result.completed, false)
    })

    it('marks quest as completed when target reached', async () => {
      await questService.getDailyQuests('complete-player')

      const result = await questService.updateProgress(
        'complete-player',
        'KillEnemies' as QuestType,
        100
      )

      ok(result)
      strictEqual(result.currentValue, 100)
      strictEqual(result.completed, true)
    })

    it('caps progress at target value', async () => {
      await questService.getDailyQuests('cap-player')

      const result = await questService.updateProgress(
        'cap-player',
        'KillEnemies' as QuestType,
        999
      )

      ok(result)
      strictEqual(result.currentValue, 100)
    })

    it('returns null for non-existent quest type', async () => {
      await questService.getDailyQuests('missing-player')

      const result = await questService.updateProgress(
        'missing-player',
        'NonExistent' as QuestType,
        50
      )

      strictEqual(result, null)
    })

    it('returns null if quest already completed', async () => {
      await questService.getDailyQuests('already-done-player')

      await questService.updateProgress('already-done-player', 'KillEnemies' as QuestType, 100)

      const result = await questService.updateProgress(
        'already-done-player',
        'KillEnemies' as QuestType,
        150
      )

      strictEqual(result, null)
    })
  })

  describe('claimQuest', () => {
    it('claims a completed quest and awards rewards', async () => {
      const quests = await questService.getDailyQuests('claim-player')
      await questService.updateProgress('claim-player', 'KillEnemies' as QuestType, 100)

      const quest = quests.find((q) => q.questType === 'KillEnemies')
      ok(quest)

      const result = await questService.claimQuest('claim-player', quest.id, false)

      ok(result)
      strictEqual(result.success, true)
      strictEqual(result.questId, quest.id)
      strictEqual(result.duskenAwarded, 500)
      strictEqual(result.bloodShardsAwarded, 0)
      strictEqual(result.adBonusApplied, false)
    })

    it('doubles rewards when ad is watched', async () => {
      const quests = await questService.getDailyQuests('ad-bonus-player')
      await questService.updateProgress('ad-bonus-player', 'KillEnemies' as QuestType, 100)

      const quest = quests.find((q) => q.questType === 'KillEnemies')
      ok(quest)

      const result = await questService.claimQuest('ad-bonus-player', quest.id, true)

      ok(result)
      strictEqual(result.duskenAwarded, 1000)
      strictEqual(result.adBonusApplied, true)
    })

    it('returns null for uncompleted quest', async () => {
      const quests = await questService.getDailyQuests('uncompleted-player')

      const quest = quests.find((q) => q.questType === 'KillEnemies')
      ok(quest)

      const result = await questService.claimQuest('uncompleted-player', quest.id, false)

      strictEqual(result, null)
    })

    it('returns null for already claimed quest', async () => {
      const quests = await questService.getDailyQuests('reclaim-player')
      await questService.updateProgress('reclaim-player', 'KillEnemies' as QuestType, 100)

      const quest = quests.find((q) => q.questType === 'KillEnemies')
      ok(quest)

      await questService.claimQuest('reclaim-player', quest.id, false)
      const result = await questService.claimQuest('reclaim-player', quest.id, false)

      strictEqual(result, null)
    })

    it('returns null for non-existent quest', async () => {
      await questService.getDailyQuests('no-quest-player')

      const result = await questService.claimQuest(
        'no-quest-player',
        '00000000-0000-0000-0000-000000000000',
        false
      )

      strictEqual(result, null)
    })
  })
})


