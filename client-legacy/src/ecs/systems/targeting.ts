import { defineQuery } from 'bitecs'

import { Faction, Health, Position, Target, ThrallTag } from '../components'
import type { GameWorld } from '../world'

export const createTargetingSystem = (world: GameWorld) => {
  const thrallQuery = defineQuery([ThrallTag, Target, Health, Position])
  const enemyQuery = defineQuery([Faction, Health, Position, Target])

  return () => {
    const thralls = thrallQuery(world).filter((entity) => Health.current[entity] > 0)
    const enemies = enemyQuery(world).filter(
      (entity) => Faction.side[entity] === 1 && Health.current[entity] > 0
    )

    const activeThrall = thralls[0] ?? null

    for (const thrall of thralls) {
      if (!enemies.length) {
        Target.eid[thrall] = 0
        continue
      }
      let closest = enemies[0]
      let shortest = Number.POSITIVE_INFINITY
      for (const enemy of enemies) {
        const dx = Position.x[enemy] - Position.x[thrall]
        const dy = Position.y[enemy] - Position.y[thrall]
        const distance = dx * dx + dy * dy
        if (distance < shortest) {
          shortest = distance
          closest = enemy
        }
      }
      Target.eid[thrall] = closest
    }

    if (!activeThrall) {
      for (const enemy of enemies) {
        Target.eid[enemy] = 0
      }
      return
    }

    for (const enemy of enemies) {
      Target.eid[enemy] = activeThrall
    }
  }
}


