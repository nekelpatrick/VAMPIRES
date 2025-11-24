import { defineQuery } from 'bitecs'

import { ActionPoints, Health, Stats } from '../components'
import type { GameWorld } from '../world'

export const createActionPointSystem = (world: GameWorld) => {
  const readyQuery = defineQuery([ActionPoints, Stats, Health])

  return (deltaSeconds: number) => {
    const entities = readyQuery(world)
    for (const entity of entities) {
      if (Health.current[entity] <= 0) {
        continue
      }
      const gain = Stats.speed[entity] * deltaSeconds * 100
      const threshold = ActionPoints.threshold[entity]
      ActionPoints.value[entity] = Math.min(
        threshold * 1.5,
        ActionPoints.value[entity] + gain
      )
    }
  }
}


