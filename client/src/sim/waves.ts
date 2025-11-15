export type EnemyStats = {
  maxHealth: number
  attack: number
  defense: number
  speed: number
  elite: boolean
}

export const computeWaveBudget = (wave: number) => ({
  totalEnemies: 4 + wave,
  maxConcurrent: Math.min(6, 3 + Math.floor(wave / 2)),
  spawnInterval: Math.max(0.6, 1.6 - wave * 0.08)
})

export const enemyStatsForWave = (wave: number, elite: boolean): EnemyStats => {
  const healthBase = 90 * Math.pow(1.08, wave - 1)
  const attackBase = 14 * Math.pow(1.05, wave - 1)
  const defenseBase = 4 + wave * 0.4
  const speedBase = 1 + wave * 0.04
  const eliteBoost = elite ? 1.5 : 1

  return {
    maxHealth: healthBase * eliteBoost,
    attack: attackBase * eliteBoost,
    defense: defenseBase * eliteBoost,
    speed: speedBase * (elite ? 1.25 : 1),
    elite
  }
}

export const rewardForEnemy = (wave: number, elite: boolean, roll: number) => {
  const baseDusken = 8 + wave * 2
  const duskenCoin = elite ? baseDusken * 2 : baseDusken
  const bloodShardChance = elite ? 0.3 : 0.05

  return {
    duskenCoin,
    bloodShards: roll < bloodShardChance ? 1 : 0
  }
}

export const bloodShardRewardForWave = (wave: number) => (wave % 10 === 0 ? 1 : 0)


