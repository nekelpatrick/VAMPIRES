import type { ThrallInstance } from '../../scene/thrall'

export const createAnimationSystem = (thrall: ThrallInstance, updateTrail: (delta: number) => void) => {
  let timer = 0

  return (deltaSeconds: number) => {
    timer += deltaSeconds
    thrall.mesh.rotation.z = Math.sin(timer * 0.5) * 0.05
    thrall.setEyeGlow(1 + Math.abs(Math.sin(timer * 2)))
    updateTrail(deltaSeconds)
  }
}

