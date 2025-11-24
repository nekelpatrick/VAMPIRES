import {
  BoxGeometry,
  Color,
  Group,
  Mesh,
  MeshBasicMaterial,
  MeshStandardMaterial,
  PlaneGeometry
} from 'three'
import type { Scene } from 'three'

type HordeVisual = {
  mesh: Group
  material: MeshStandardMaterial
  hpBar: Mesh
  hpFill: Mesh
}

type SpawnOptions = {
  elite: boolean
  x: number
  z: number
  height: number
}

export const createHordeVisuals = (scene: Scene) => {
  const fodderGeometries = [
    new BoxGeometry(0.3, 1.4, 0.25),
    new BoxGeometry(0.35, 1.2, 0.25),
    new BoxGeometry(0.28, 1.1, 0.25)
  ]
  const shadowGeometry = new PlaneGeometry(1.2, 0.5)
  const shadowMaterial = new MeshBasicMaterial({
    color: 0x000000,
    transparent: true,
    opacity: 0.35
  })
  const visuals = new Map<number, HordeVisual>()

  return {
    spawn(entity: number, options: SpawnOptions) {
      const group = new Group()
      const variant = entity % fodderGeometries.length
      const palette = options.elite
        ? { color: 0xff6b6b, emissive: 0xce1d3b }
        : [{ color: 0x79404b, emissive: 0x2b0b16 }, { color: 0x6c3c2e, emissive: 0x260c08 }, { color: 0x5b3350, emissive: 0x1d0617 }][variant]
      const material = new MeshStandardMaterial({
        color: palette.color,
        emissive: new Color(palette.emissive),
        emissiveIntensity: options.elite ? 1.3 : 0.6,
        roughness: options.elite ? 0.2 : 0.35,
        metalness: 0.25
      })
      const bodyGeometry = options.elite ? new BoxGeometry(0.4, 1.8, 0.3) : fodderGeometries[variant]
      const body = new Mesh(bodyGeometry, material)
      group.add(body)

      const hpBack = new Mesh(new PlaneGeometry(0.9, 0.09), new MeshBasicMaterial({ color: 0x12040a }))
      hpBack.position.set(0, 0.8, 0)
      group.add(hpBack)

      const hpFill = new Mesh(
        new PlaneGeometry(0.88, 0.07),
        new MeshBasicMaterial({ color: options.elite ? 0xff8c42 : 0x32e37b })
      )
      hpFill.position.set(0, 0.8, 0.01)
      hpFill.scale.x = 1
      group.add(hpFill)
      group.position.set(options.x, -0.6 + options.height, options.z)

      const shadow = new Mesh(shadowGeometry, shadowMaterial.clone())
      shadow.rotation.x = -Math.PI / 2
      shadow.position.set(0, -0.5, 0)
      group.add(shadow)

      if (options.elite) {
        const hornMaterial = material.clone()
        hornMaterial.emissiveIntensity = 1.6
        ;[-0.35, 0.35].forEach((x) => {
          const horn = new Mesh(new BoxGeometry(0.08, 0.5, 0.08), hornMaterial)
          horn.position.set(x, 0.6, 0)
          group.add(horn)
        })
      }

      scene.add(group)
      visuals.set(entity, { mesh: group, material, hpBar: hpBack, hpFill })
    },
    updateHealth(entity: number, ratio: number) {
      const visual = visuals.get(entity)
      if (!visual) {
        return
      }
      visual.mesh.scale.setScalar(0.8 + ratio * 0.5)
      visual.material.emissiveIntensity = 0.3 + ratio
      visual.hpFill.scale.x = Math.max(0, Math.min(1, ratio))
      visual.hpFill.position.x = (visual.hpFill.scale.x - 1) * 0.44
    },
    setPosition(entity: number, x: number, y: number, z: number) {
      const visual = visuals.get(entity)
      if (!visual) {
        return
      }
      visual.mesh.position.set(x, y, z)
    },
    remove(entity: number) {
      const visual = visuals.get(entity)
      if (!visual) {
        return
      }
      scene.remove(visual.mesh)
      visual.mesh.traverse((child) => {
        if ((child as Mesh).isMesh) {
          const meshChild = child as Mesh
          meshChild.geometry?.dispose()
          const materials = Array.isArray(meshChild.material)
            ? meshChild.material
            : [meshChild.material]
          materials.forEach((material) => material?.dispose())
        }
      })
      visuals.delete(entity)
    }
  }
}


