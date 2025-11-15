import { addComponent, addEntity } from 'bitecs'
import type { Scene } from 'three'
import {
  BoxGeometry,
  Group,
  Mesh,
  MeshStandardMaterial,
  SphereGeometry,
  Vector3
} from 'three'

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

  const armorMaterial = new MeshStandardMaterial({
    color: 0x2a2d34,
    emissive: 0x120b13,
    roughness: 0.6,
    metalness: 0.2
  })

  const body = new Mesh(new BoxGeometry(1.2, 2.6, 0.8), armorMaterial)
  body.position.y = 0.5
  thrallGroup.add(body)

  const head = new Mesh(new BoxGeometry(1.1, 1, 0.9), armorMaterial)
  head.position.y = 1.8
  thrallGroup.add(head)

  const eyeMaterial = new MeshStandardMaterial({
    color: 0xff4d4d,
    emissive: 0xff1c1c,
    emissiveIntensity: 1.5
  })

  const leftEye = new Mesh(new SphereGeometry(0.12, 16, 16), eyeMaterial.clone())
  leftEye.position.set(-0.25, 1.9, 0.35)
  const rightEye = leftEye.clone()
  rightEye.position.x = 0.25
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

