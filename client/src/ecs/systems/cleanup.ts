import { defineQuery, removeEntity } from 'bitecs'

import { Faction, Health } from '../components'
import type { GameWorld } from '../world'

type HordeVisuals = {
  remove: (entity: number) => void
}

export const createCleanupSystem = (world: GameWorld, hordeVisuals: HordeVisuals) => {
  const enemyQuery = defineQuery([Faction, Health])

  return () => {
    const entities = enemyQuery(world)
    for (const entity of entities) {
      if (Faction.side[entity] !== 1) {
        continue
      }
      if (Health.current[entity] > 0) {
        continue
      }
      hordeVisuals.remove(entity)
      removeEntity(world, entity)
    }
  }
}



