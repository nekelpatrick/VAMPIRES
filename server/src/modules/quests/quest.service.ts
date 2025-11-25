import { randomUUID } from 'crypto'
import type { Pool } from 'pg'

import { getPool } from '../../lib/db'
import { economyService } from '../economy'
import type {
  QuestProgress,
  QuestType,
  ClaimQuestResponse,
  QuestDefinition
} from './quest.schema'

const DAILY_QUEST_DEFINITIONS: QuestDefinition[] = [
  {
    questType: 'KillEnemies',
    questName: 'Slay the Horde',
    description: 'Kill 100 enemies',
    targetValue: 100,
    duskenReward: 500,
    bloodShardsReward: 0
  },
  {
    questType: 'ReachWave',
    questName: 'Wave Crusher',
    description: 'Reach wave 20',
    targetValue: 20,
    duskenReward: 1000,
    bloodShardsReward: 0
  },
  {
    questType: 'DealDamage',
    questName: 'Blood Harvest',
    description: 'Deal 50,000 damage',
    targetValue: 50000,
    duskenReward: 750,
    bloodShardsReward: 0
  },
  {
    questType: 'WinPvP',
    questName: 'PvP Victor',
    description: 'Win 3 PvP battles',
    targetValue: 3,
    duskenReward: 0,
    bloodShardsReward: 2
  },
  {
    questType: 'GetCombo',
    questName: 'Combo Master',
    description: 'Get 50x combo',
    targetValue: 50,
    duskenReward: 300,
    bloodShardsReward: 0
  }
]

export class QuestService {
  private pool: Pool | null = null
  private questCache: Map<string, QuestProgress[]> = new Map()

  constructor() {
    try {
      this.pool = getPool()
    } catch {
      console.log('[QuestService] Running without database')
    }
  }

  async getDailyQuests(playerId: string): Promise<QuestProgress[]> {
    const today = new Date().toISOString().split('T')[0]
    const cacheKey = `${playerId}_${today}`

    if (this.questCache.has(cacheKey)) {
      return this.questCache.get(cacheKey)!
    }

    if (this.pool) {
      const result = await this.pool.query(
        `SELECT * FROM daily_quests WHERE player_id = $1 AND quest_date = $2`,
        [playerId, today]
      )

      if (result.rows.length > 0) {
        const quests = result.rows.map(this.mapRowToQuest)
        this.questCache.set(cacheKey, quests)
        return quests
      }
    }

    return this.createDailyQuests(playerId, today)
  }

  private async createDailyQuests(playerId: string, date: string): Promise<QuestProgress[]> {
    const quests: QuestProgress[] = []

    for (const def of DAILY_QUEST_DEFINITIONS) {
      const quest: QuestProgress = {
        id: randomUUID(),
        playerId,
        questType: def.questType,
        questName: def.questName,
        description: def.description,
        targetValue: def.targetValue,
        currentValue: 0,
        duskenReward: def.duskenReward,
        bloodShardsReward: def.bloodShardsReward,
        completed: false,
        claimed: false,
        adBonusClaimed: false,
        questDate: date
      }

      if (this.pool) {
        await this.pool.query(
          `INSERT INTO daily_quests (id, player_id, quest_type, quest_name, description, target_value, current_value, dusken_reward, blood_shards_reward, quest_date)
           VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)`,
          [
            quest.id,
            quest.playerId,
            quest.questType,
            quest.questName,
            quest.description,
            quest.targetValue,
            quest.currentValue,
            quest.duskenReward,
            quest.bloodShardsReward,
            quest.questDate
          ]
        )
      }

      quests.push(quest)
    }

    const cacheKey = `${playerId}_${date}`
    this.questCache.set(cacheKey, quests)

    return quests
  }

  async updateProgress(
    playerId: string,
    questType: QuestType,
    value: number
  ): Promise<QuestProgress | null> {
    const quests = await this.getDailyQuests(playerId)
    const quest = quests.find((q) => q.questType === questType && !q.completed)

    if (!quest) return null

    quest.currentValue = Math.min(value, quest.targetValue)

    if (quest.currentValue >= quest.targetValue) {
      quest.completed = true
    }

    if (this.pool) {
      await this.pool.query(
        `UPDATE daily_quests SET current_value = $1, completed = $2, updated_at = NOW() WHERE id = $3`,
        [quest.currentValue, quest.completed, quest.id]
      )
    }

    return quest
  }

  async claimQuest(
    playerId: string,
    questId: string,
    watchedAd: boolean
  ): Promise<ClaimQuestResponse | null> {
    const quests = await this.getDailyQuests(playerId)
    const quest = quests.find((q) => q.id === questId)

    if (!quest || !quest.completed || quest.claimed) {
      return null
    }

    let duskenAwarded = quest.duskenReward
    let bloodShardsAwarded = quest.bloodShardsReward
    const adBonusApplied = watchedAd && !quest.adBonusClaimed

    if (adBonusApplied) {
      duskenAwarded *= 2
      bloodShardsAwarded *= 2
      quest.adBonusClaimed = true
    }

    quest.claimed = true

    if (duskenAwarded > 0) {
      await economyService.addDuskenCoin(playerId, duskenAwarded, 'quest_reward')
    }

    if (bloodShardsAwarded > 0) {
      await economyService.addBloodShards(playerId, bloodShardsAwarded, 'quest_reward')
    }

    if (this.pool) {
      await this.pool.query(
        `UPDATE daily_quests SET claimed = $1, ad_bonus_claimed = $2, updated_at = NOW() WHERE id = $3`,
        [quest.claimed, quest.adBonusClaimed, quest.id]
      )
    }

    return {
      success: true,
      questId: quest.id,
      duskenAwarded,
      bloodShardsAwarded,
      adBonusApplied
    }
  }

  private mapRowToQuest(row: Record<string, unknown>): QuestProgress {
    return {
      id: row.id as string,
      playerId: row.player_id as string,
      questType: row.quest_type as QuestType,
      questName: row.quest_name as string,
      description: row.description as string | null,
      targetValue: row.target_value as number,
      currentValue: row.current_value as number,
      duskenReward: row.dusken_reward as number,
      bloodShardsReward: row.blood_shards_reward as number,
      completed: row.completed as boolean,
      claimed: row.claimed as boolean,
      adBonusClaimed: row.ad_bonus_claimed as boolean,
      questDate: (row.quest_date as Date).toISOString().split('T')[0]
    }
  }
}

export const questService = new QuestService()

