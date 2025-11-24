import { createWorld } from 'bitecs'
import type { IWorld } from 'bitecs'

import { SeededRng } from '../sim/seededRng'

export type CombatPhase = 'spawning' | 'fighting' | 'wave-clear' | 'defeat'

export type GameWorld = IWorld & {
  combat: {
    phase: CombatPhase
    wave: number
    kills: number
    rng: SeededRng
    pendingDuskenCoin: number
    pendingBloodShards: number
    waveBudget: {
      totalEnemies: number
      spawned: number
      cleared: number
      spawnTimer: number
      maxConcurrent: number
      spawnInterval: number
    }
  }
}

export const createGameWorld = (seed: number): GameWorld => {
  const world = createWorld() as GameWorld
  world.combat = {
    phase: 'spawning',
    wave: 1,
    kills: 0,
    rng: new SeededRng(seed),
    pendingDuskenCoin: 0,
    pendingBloodShards: 0,
    waveBudget: {
      totalEnemies: 0,
      spawned: 0,
      cleared: 0,
      spawnTimer: 0,
      maxConcurrent: 3,
      spawnInterval: 1.2
    }
  }
  return world
}


