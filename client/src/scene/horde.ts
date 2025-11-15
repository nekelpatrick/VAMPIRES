import {
  Color,
  Group,
  Mesh,
  MeshStandardMaterial,
  SphereGeometry,
  Vector3
} from 'three'
import type { Scene } from 'three'

type HordeVisual = {
  mesh: Group
  material: MeshStandardMaterial
}

type SpawnOptions = {
  elite: boolean
  offset: number
  height: number
}

export const createHordeVisuals = (scene: Scene) => {
  const geometry = new SphereGeometry(0.6, 24, 24)
  const visuals = new Map<number, HordeVisual>()

  return {
    spawn(entity: number, options: SpawnOptions) {
      const group = new Group()
      const material = new MeshStandardMaterial({
        color: options.elite ? 0xff6b6b : 0x8b2c2c,
        emissive: options.elite ? new Color(0xce1d3b) : new Color(0x4c111a),
        emissiveIntensity: options.elite ? 1.2 : 0.6,
        roughness: 0.4,
        metalness: 0.2
      })
      const body = new Mesh(geometry, material)
      group.add(body)
      group.position.copy(new Vector3(2.2 + options.offset, -0.6 + options.height, 0))
      scene.add(group)
      visuals.set(entity, { mesh: group, material })
    },
    updateHealth(entity: number, ratio: number) {
      const visual = visuals.get(entity)
      if (!visual) {
        return
      }
      visual.mesh.scale.setScalar(0.8 + ratio * 0.5)
      visual.material.emissiveIntensity = 0.3 + ratio
    },
    remove(entity: number) {
      const visual = visuals.get(entity)
      if (!visual) {
        return
      }
      scene.remove(visual.mesh)
      visual.mesh.traverse((child) => {
        if ('geometry' in child && child.geometry) {
          child.geometry.dispose()
        }
        if ('material' in child && child.material) {
          const mats = Array.isArray(child.material) ? child.material : [child.material]
          mats.forEach((mat) => mat.dispose())
        }
      })
      visuals.delete(entity)
    }
  }
}


