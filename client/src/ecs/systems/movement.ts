import { defineQuery } from 'bitecs'

import type { ThrallInstance } from '../../scene/thrall'
import { ThrallTag, Position, Velocity } from '../components'
import type { GameWorld } from '../world'

export const createMovementSystem = (world: GameWorld, thrall: ThrallInstance) => {
  const thrallQuery = defineQuery([ThrallTag, Position, Velocity])

  return (deltaSeconds: number) => {
    const entities = thrallQuery(world)

    for (const entity of entities) {
      Position.x[entity] += Velocity.x[entity] * deltaSeconds
      Position.y[entity] += Velocity.y[entity] * deltaSeconds
      Position.z[entity] += Velocity.z[entity] * deltaSeconds

      if (Position.x[entity] > 3.5 || Position.x[entity] < -3.5) {
        Velocity.x[entity] = -Velocity.x[entity]
      }

      thrall.mesh.position.set(Position.x[entity], Position.y[entity], Position.z[entity])
    }
  }
}

