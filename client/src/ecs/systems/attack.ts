import { defineQuery } from 'bitecs'
import type { StoreApi } from 'zustand/vanilla'

import type { ThrallInstance } from '../../scene/thrall'
import type { HudState } from '../../services/state'
import { applyVariance, computeDamage } from '../../sim/combatMath'
import {
  ActionPoints,
  Faction,
  Health,
  Reward,
  Stats,
  Target,
  ThrallTag
} from '../components'
import type { GameWorld } from '../world'

type HordeVisuals = {
  updateHealth: (entity: number, ratio: number) => void
}

export const createAttackSystem = (
  world: GameWorld,
  hudStore: StoreApi<HudState>,
  thrall: ThrallInstance,
  hordeVisuals: HordeVisuals
) => {
  const attackQuery = defineQuery([ActionPoints, Target, Stats, Health, Faction])
  const thrallQuery = defineQuery([ThrallTag, Health])

  return () => {
    const entities = attackQuery(world)
    for (const entity of entities) {
      if (ActionPoints.value[entity] < ActionPoints.threshold[entity]) {
        continue
      }
      const target = Target.eid[entity]
      if (!target || Health.current[target] <= 0) {
        continue
      }

      const baseDamage = computeDamage(Stats.attack[entity], Stats.defense[target])
      const roll = world.combat.rng.nextFloat()
      const damage = applyVariance(baseDamage, 0.2, roll)

      Health.current[target] = Math.max(0, Health.current[target] - damage)
      ActionPoints.value[entity] = 0

      if (Faction.side[target] === 1) {
        const maxHealth = Health.max[target] || 1
        hordeVisuals.updateHealth(target, Health.current[target] / maxHealth)
      } else {
        const thrallHealth = Health.current[target]
        const thrallMax = Health.max[target] || 1
        const ratio = thrallHealth / thrallMax
        thrall.setEyeGlow(0.4 + ratio)
      }

      if (Health.current[target] > 0) {
        continue
      }

      if (Faction.side[target] === 1) {
        world.combat.kills += 1
        world.combat.waveBudget.cleared += 1
        const duskenCoin = Reward.duskenCoin[target] ?? 0
        const bloodShards = Reward.bloodShards[target] ?? 0
        world.combat.pendingDuskenCoin += duskenCoin
        world.combat.pendingBloodShards += bloodShards
        hudStore.getState().applyCombat({
          kills: world.combat.kills,
          pendingDuskenCoin: world.combat.pendingDuskenCoin,
          pendingBloodShards: world.combat.pendingBloodShards
        })
        if (bloodShards > 0) {
          hudStore.getState().applyCurrencyDelta({
            duskenCoin: duskenCoin,
            bloodShards
          })
        } else {
          hudStore.getState().applyCurrencyDelta({ duskenCoin })
        }
      } else {
        world.combat.phase = 'defeat'
        hudStore.getState().setCombat({ thrallAlive: false })
        const thrallEntity = thrallQuery(world)[0]
        if (thrallEntity) {
          Target.eid[thrallEntity] = 0
        }
      }
    }
  }
}


