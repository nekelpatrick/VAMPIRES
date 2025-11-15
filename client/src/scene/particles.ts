import { BufferGeometry, Float32BufferAttribute, Points, PointsMaterial, Vector3 } from 'three'
import type { Scene } from 'three'

import type { ThrallInstance } from './thrall'

export const createBloodTrail = (scene: Scene, thrall: ThrallInstance) => {
  const particleCount = 40
  const geometry = new BufferGeometry()
  const positions = new Float32Array(particleCount * 3)
  geometry.setAttribute('position', new Float32BufferAttribute(positions, 3))

  const material = new PointsMaterial({
    color: 0xb00b1e,
    size: 0.08,
    transparent: true,
    opacity: 0.85,
    depthWrite: false
  })

  const points = new Points(geometry, material)
  points.position.copy(thrall.mesh.position)
  scene.add(points)

  const target = new Vector3()

  return (deltaSeconds: number) => {
    target.copy(thrall.mesh.position)
    points.position.copy(target)

    for (let i = 0; i < particleCount; i += 1) {
      const index = i * 3
      positions[index] += (Math.random() - 0.5) * deltaSeconds * 2
      positions[index + 1] = Math.sin((positions[index] + performance.now() * 0.001) + i) * 0.3
      positions[index + 2] += (Math.random() - 0.5) * deltaSeconds * 2

      if (positions[index] > 1 || positions[index] < -1) {
        positions[index] = (Math.random() - 0.5) * 0.5
      }
    }

    geometry.attributes.position.needsUpdate = true
  }
}

