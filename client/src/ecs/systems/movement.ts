import { defineQuery } from 'bitecs'

import type { ThrallInstance } from '../../scene/thrall'
import { ActionPoints, Faction, Health, Position, Stats, Target, ThrallTag, Velocity } from '../components'
import type { GameWorld } from '../world'

type HordeVisuals = {
  setPosition: (entity: number, x: number, y: number, z: number) => void
}

const clamp = (value: number, min: number, max: number) => Math.min(max, Math.max(min, value))

export const createMovementSystem = (
  world: GameWorld,
  thrall: ThrallInstance,
  hordeVisuals: HordeVisuals
) => {
  const moverQuery = defineQuery([Position, Velocity, Target, Stats, Health, Faction])
  const thrallQuery = defineQuery([ThrallTag])

  return (deltaSeconds: number) => {
    const movers = moverQuery(world)
    for (const entity of movers) {
      if (Health.current[entity] <= 0) {
        Velocity.x[entity] = 0
        Velocity.z[entity] = 0
        continue
      }

      const target = Target.eid[entity]
      const stopRange = Faction.side[entity] === 0 ? 0.65 : 0.55
      let distance = Infinity
      let dirX = 0
      let dirZ = 0

      if (target && Health.current[target] > 0) {
        const dx = Position.x[target] - Position.x[entity]
        const dz = Position.z[target] - Position.z[entity]
        distance = Math.hypot(dx, dz) || 0.0001
        dirX = dx / distance
        dirZ = dz / distance
      }

      if (distance > stopRange) {
        const baseSpeed = Stats.speed[entity] * 1.1
        Velocity.x[entity] = dirX * baseSpeed
        Velocity.z[entity] = dirZ * baseSpeed
      } else {
        Velocity.x[entity] *= 0.6
        Velocity.z[entity] *= 0.6
      }

      Position.x[entity] = clamp(Position.x[entity] + Velocity.x[entity] * deltaSeconds, -6, 6)
      Position.z[entity] = clamp(Position.z[entity] + Velocity.z[entity] * deltaSeconds, -3, 3)

      if (Faction.side[entity] === 0) {
        thrall.mesh.position.set(Position.x[entity], Position.y[entity], Position.z[entity])
      } else {
        hordeVisuals.setPosition(entity, Position.x[entity], Position.y[entity], Position.z[entity])
      }
    }

    const thrallEntities = thrallQuery(world)
    for (const entity of thrallEntities) {
      const target = Target.eid[entity]
      if (!target) {
        continue
      }
      const dx = Position.x[target] - Position.x[entity]
      const dz = Position.z[target] - Position.z[entity]
      const distance = Math.hypot(dx, dz)
      if (distance > 0.2) {
        continue
      }
      ActionPoints.value[entity] += Stats.speed[entity] * deltaSeconds * 50
    }
  }
}

