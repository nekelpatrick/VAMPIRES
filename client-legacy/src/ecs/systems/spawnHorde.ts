import { addComponent, addEntity, defineQuery } from 'bitecs'
import type { StoreApi } from 'zustand/vanilla'

import type { HudState } from '../../services/state'
import {
  bloodShardRewardForWave,
  computeWaveBudget,
  enemyStatsForWave,
  rewardForEnemy
} from '../../sim/waves'
import {
  ActionPoints,
  Faction,
  Health,
  Position,
  Reward,
  SpawnTag,
  Stats,
  Target,
  Velocity
} from '../components'
import type { GameWorld } from '../world'

type HordeVisuals = {
  spawn: (entity: number, options: { elite: boolean; x: number; z: number; height: number }) => void
  updateHealth: (entity: number, ratio: number) => void
  setPosition: (entity: number, x: number, y: number, z: number) => void
  remove: (entity: number) => void
}

export const createSpawnHordeSystem = (
  world: GameWorld,
  hudStore: StoreApi<HudState>,
  hordeVisuals: HordeVisuals,
  onWaveCleared: () => void
) => {
  const enemyQuery = defineQuery([Faction, Health])

  const ensureBudget = () => {
    if (world.combat.waveBudget.totalEnemies > 0) {
      return
    }
    const budget = computeWaveBudget(world.combat.wave)
    world.combat.waveBudget = {
      totalEnemies: budget.totalEnemies,
      spawned: 0,
      cleared: 0,
      spawnTimer: 0,
      maxConcurrent: budget.maxConcurrent,
      spawnInterval: budget.spawnInterval
    }
    hudStore.getState().setCombat({
      wave: world.combat.wave,
      thrallAlive: true
    })
  }

  const spawnEnemy = () => {
    const isElite = (world.combat.wave + world.combat.waveBudget.spawned) % 5 === 0
    const stats = enemyStatsForWave(world.combat.wave, isElite)
    const entity = addEntity(world)
    addComponent(world, Position, entity)
    addComponent(world, Stats, entity)
    addComponent(world, Health, entity)
    addComponent(world, ActionPoints, entity)
    addComponent(world, Faction, entity)
    addComponent(world, Target, entity)
    addComponent(world, SpawnTag, entity)
    addComponent(world, Reward, entity)
    addComponent(world, Velocity, entity)

    const spawnSide = world.combat.rng.nextFloat() > 0.5 ? 1 : -1
    const offsetX = world.combat.rng.range(-0.5, 3.5)
    const spawnZ = world.combat.rng.range(-2.5, 2.5)
    const height = world.combat.rng.range(0, 0.8)

    const spawnX = spawnSide * (2.2 + Math.abs(offsetX))
    Position.x[entity] = spawnX
    Position.y[entity] = -0.6 + height * 0.5
    Position.z[entity] = spawnZ

    Stats.attack[entity] = stats.attack
    Stats.defense[entity] = stats.defense
    Stats.speed[entity] = stats.speed
    Stats.critChance[entity] = isElite ? 0.2 : 0.05

    Health.current[entity] = stats.maxHealth
    Health.max[entity] = stats.maxHealth

    ActionPoints.value[entity] = 0
    ActionPoints.threshold[entity] = 100
    Faction.side[entity] = 1
    Target.eid[entity] = 0
    SpawnTag.wave[entity] = world.combat.wave
    SpawnTag.elite[entity] = isElite ? 1 : 0
    Velocity.x[entity] = 0
    Velocity.y[entity] = 0
    Velocity.z[entity] = 0

    const reward = rewardForEnemy(world.combat.wave, isElite, world.combat.rng.nextFloat())
    Reward.duskenCoin[entity] = reward.duskenCoin
    Reward.bloodShards[entity] = reward.bloodShards

    hordeVisuals.spawn(entity, { elite: isElite, x: spawnX, z: spawnZ, height: height * 0.5 })
    world.combat.waveBudget.spawned += 1
    world.combat.phase = 'fighting'
  }

  return (deltaSeconds: number) => {
    if (world.combat.phase === 'defeat') {
      return
    }

    ensureBudget()

    world.combat.waveBudget.spawnTimer += deltaSeconds
    const activeEnemies = enemyQuery(world).filter(
      (entity) => Faction.side[entity] === 1 && Health.current[entity] > 0
    )

    if (
      world.combat.waveBudget.spawned < world.combat.waveBudget.totalEnemies &&
      activeEnemies.length < world.combat.waveBudget.maxConcurrent &&
      world.combat.waveBudget.spawnTimer >= world.combat.waveBudget.spawnInterval
    ) {
      spawnEnemy()
      world.combat.waveBudget.spawnTimer = 0
    }

    if (
      world.combat.waveBudget.cleared >= world.combat.waveBudget.totalEnemies &&
      activeEnemies.length === 0
    ) {
      world.combat.phase = 'wave-clear'
      const clearedWave = world.combat.wave
      const bonusBlood = bloodShardRewardForWave(clearedWave)
      if (bonusBlood > 0) {
        world.combat.pendingBloodShards += bonusBlood
        hudStore.getState().applyCurrencyDelta({ bloodShards: bonusBlood })
        hudStore.getState().applyCombat({
          pendingBloodShards: world.combat.pendingBloodShards
        })
      }
      world.combat.wave += 1
      world.combat.waveBudget = {
        ...world.combat.waveBudget,
        totalEnemies: 0,
        spawned: 0,
        cleared: 0,
        spawnTimer: 0
      }
      hudStore.getState().setCombat({ wave: world.combat.wave })
      onWaveCleared()
    }
  }
}


