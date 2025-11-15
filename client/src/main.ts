import './styles/index.css'

import { createActionPointSystem } from './ecs/systems/actionPoints'
import { createAnimationSystem } from './ecs/systems/animation'
import { createAttackSystem } from './ecs/systems/attack'
import { createCleanupSystem } from './ecs/systems/cleanup'
import { createMovementSystem } from './ecs/systems/movement'
import { createSpawnHordeSystem } from './ecs/systems/spawnHorde'
import { createTargetingSystem } from './ecs/systems/targeting'
import { createGameWorld } from './ecs/world'
import { createHud } from './hud/currencies'
import { createHordeVisuals } from './scene/horde'
import { createBloodTrail } from './scene/particles'
import { createScene } from './scene/renderer'
import { spawnThrall } from './scene/thrall'
import { fetchHealth, fetchPlayerProfile, syncWallet } from './services/api'
import { createHudStore } from './services/state'
import { FixedTicker } from './sim/ticker'

const root = document.querySelector<HTMLDivElement>('#app')
if (!root) {
  throw new Error('Root element #app not found')
}

const canvas = document.createElement('canvas')
canvas.id = 'battlefield'
root.appendChild(canvas)

const hudRoot = document.createElement('div')
root.appendChild(hudRoot)

const seedBuffer = new Uint32Array(1)
crypto.getRandomValues(seedBuffer)
const world = createGameWorld(seedBuffer[0] || Date.now())
const { renderer, scene, camera, updateCameraFocus } = createScene(canvas)
const thrall = spawnThrall(world, scene)
const updateBloodTrail = createBloodTrail(scene, thrall)
const hordeVisuals = createHordeVisuals(scene)

const movementSystem = createMovementSystem(world, thrall, hordeVisuals)
const animationSystem = createAnimationSystem(thrall, updateBloodTrail)
const actionPointsSystem = createActionPointSystem(world)
const targetingSystem = createTargetingSystem(world)

const hudStore = createHudStore()
createHud(hudRoot, hudStore)

const attackSystem = createAttackSystem(world, hudStore, thrall, hordeVisuals)
const cleanupSystem = createCleanupSystem(world, hordeVisuals)

const playerId = 'demo-player'

const flushCurrencyQueue = async () => {
  const state = hudStore.getState()
  if (state.pendingDuskenCoin === 0 && state.pendingBloodShards === 0) {
    return
  }
  try {
    const wallet = await syncWallet({
      playerId,
      duskenCoinDelta: state.pendingDuskenCoin,
      bloodShardDelta: state.pendingBloodShards
    })
    hudStore.getState().markSynced({
      duskenCoin: wallet.duskenCoinBalance,
      bloodShards: wallet.bloodShardBalance
    })
  } catch (error) {
    console.error('[hud] Failed to sync wallet delta', error)
    hudStore.getState().applyCombat({
      pendingDuskenCoin: state.pendingDuskenCoin,
      pendingBloodShards: state.pendingBloodShards
    })
  }
}

const spawnSystem = createSpawnHordeSystem(world, hudStore, hordeVisuals, () => {
  void flushCurrencyQueue()
})

const ticker = new FixedTicker(100, (stepSeconds) => {
  actionPointsSystem(stepSeconds)
  targetingSystem()
  attackSystem()
  cleanupSystem()
  spawnSystem(stepSeconds)
})

let previous = performance.now()
const animate = (now: number) => {
  const deltaSeconds = (now - previous) / 1000
  previous = now

  ticker.update(deltaSeconds)
  movementSystem(deltaSeconds)
  updateCameraFocus(thrall.mesh.position.x)
  animationSystem(deltaSeconds)
  renderer.render(scene, camera)

  requestAnimationFrame(animate)
}

requestAnimationFrame(animate)

setInterval(() => {
  void flushCurrencyQueue()
}, 5000)

void fetchHealth()

const hydrateProfile = async () => {
  const profile = await fetchPlayerProfile()
  hudStore
    .getState()
    .setResources({
      thrallName: profile.displayName,
      duskenCoin: profile.duskenCoinBalance,
      bloodShards: profile.bloodShardBalance,
      thrallHp: 320,
      thrallHpMax: 320
    })
}

void hydrateProfile()
