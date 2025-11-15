import './styles/index.css'

import { createAnimationSystem } from './ecs/systems/animation'
import { createMovementSystem } from './ecs/systems/movement'
import { createGameWorld } from './ecs/world'
import { createHud } from './hud/currencies'
import { createBloodTrail } from './scene/particles'
import { createScene } from './scene/renderer'
import { spawnThrall } from './scene/thrall'
import { fetchHealth, fetchPlayerProfile } from './services/api'
import { createHudStore } from './services/state'

const root = document.querySelector<HTMLDivElement>('#app')
if (!root) {
  throw new Error('Root element #app not found')
}

const canvas = document.createElement('canvas')
canvas.id = 'battlefield'
root.appendChild(canvas)

const hudRoot = document.createElement('div')
root.appendChild(hudRoot)

const world = createGameWorld()
const { renderer, scene, camera } = createScene(canvas)
const thrall = spawnThrall(world, scene)
const updateBloodTrail = createBloodTrail(scene, thrall)

const movementSystem = createMovementSystem(world, thrall)
const animationSystem = createAnimationSystem(thrall, updateBloodTrail)

const hudStore = createHudStore()
createHud(hudRoot, hudStore)

let previous = performance.now()
const animate = (now: number) => {
  const deltaSeconds = (now - previous) / 1000
  previous = now

  movementSystem(deltaSeconds)
  animationSystem(deltaSeconds)
  renderer.render(scene, camera)

  requestAnimationFrame(animate)
}

requestAnimationFrame(animate)

void fetchHealth()

const hydrateProfile = async () => {
  const profile = await fetchPlayerProfile()
  hudStore
    .getState()
    .setResources({
      thrallName: profile.displayName,
      duskenCoin: profile.duskenCoinBalance,
      bloodShards: profile.bloodShardBalance
    })
}

void hydrateProfile()
