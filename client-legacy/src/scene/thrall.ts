import { addComponent, addEntity } from 'bitecs'
import type { Scene } from 'three'
import { BoxGeometry, Group, Mesh, MeshStandardMaterial, Vector3 } from 'three'

import {
  ActionPoints,
  Faction,
  Health,
  Position,
  Stats,
  Target,
  ThrallTag,
  Velocity
} from '../ecs/components'
import type { GameWorld } from '../ecs/world'

export type ThrallInstance = {
  entity: number
  mesh: Group
  setEyeGlow: (intensity: number) => void
}

export const spawnThrall = (world: GameWorld, scene: Scene): ThrallInstance => {
  const thrallGroup = new Group()
  thrallGroup.position.copy(new Vector3(-3.2, -0.8, 0))

  const bodyMaterial = new MeshStandardMaterial({
    color: 0xf26b1d,
    emissive: 0x221105,
    roughness: 0.4,
    metalness: 0.15
  })

  const body = new Mesh(new BoxGeometry(0.4, 1.5, 0.3), bodyMaterial)
  body.position.y = -0.05
  thrallGroup.add(body)

  const trimMaterial = new MeshStandardMaterial({
    color: 0x110509,
    emissive: 0x110509,
    roughness: 0.6
  })

  const trim = new Mesh(new BoxGeometry(0.45, 1.6, 0.08), trimMaterial)
  trim.position.set(0, 0, 0.2)
  thrallGroup.add(trim)

  const eyeMaterial = new MeshStandardMaterial({
    color: 0xffee9a,
    emissive: 0xffee9a,
    emissiveIntensity: 1.4
  })

  const leftEye = new Mesh(new BoxGeometry(0.12, 0.25, 0.05), eyeMaterial)
  leftEye.position.set(-0.12, 0.4, 0.22)
  const rightEye = leftEye.clone()
  rightEye.position.x = 0.18
  thrallGroup.add(leftEye, rightEye)

  scene.add(thrallGroup)

  const entity = addEntity(world)
  addComponent(world, Position, entity)
  addComponent(world, Velocity, entity)
  addComponent(world, ThrallTag, entity)
  addComponent(world, Stats, entity)
  addComponent(world, Health, entity)
  addComponent(world, ActionPoints, entity)
  addComponent(world, Faction, entity)
  addComponent(world, Target, entity)

  Position.x[entity] = thrallGroup.position.x
  Position.y[entity] = thrallGroup.position.y
  Position.z[entity] = thrallGroup.position.z

  Velocity.x[entity] = 0.35
  Velocity.y[entity] = 0
  Velocity.z[entity] = 0

  Stats.attack[entity] = 52
  Stats.defense[entity] = 22
  Stats.speed[entity] = 1.3
  Stats.critChance[entity] = 0.15

  Health.current[entity] = 320
  Health.max[entity] = 320

  ActionPoints.value[entity] = 0
  ActionPoints.threshold[entity] = 100

  Faction.side[entity] = 0
  Target.eid[entity] = 0

  return {
    entity,
    mesh: thrallGroup,
    setEyeGlow: (intensity: number) => {
      leftEye.material.emissiveIntensity = intensity
      rightEye.material.emissiveIntensity = intensity
    }
  }
}

