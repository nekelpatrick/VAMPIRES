import { describe, expect, it } from 'vitest'

import { computeDamage, applyVariance } from '../sim/combatMath'
import { SeededRng } from '../sim/seededRng'
import { FixedTicker } from '../sim/ticker'
import { rewardForEnemy } from '../sim/waves'

describe('simulation helpers', () => {
  it('produces deterministic rng sequences', () => {
    const a = new SeededRng(42)
    const b = new SeededRng(42)
    const seqA = Array.from({ length: 5 }, () => a.next())
    const seqB = Array.from({ length: 5 }, () => b.next())
    expect(seqA).toEqual(seqB)
  })

  it('computes damage with variance applied', () => {
    const damage = computeDamage(80, 20)
    const varied = applyVariance(damage, 0.2, 0.75)
    expect(damage).toBeGreaterThan(0)
    expect(varied).toBeGreaterThanOrEqual(Math.floor(damage * 0.9))
  })

  it('ticks fixed intervals at consistent cadence', () => {
    let ticks = 0
    const ticker = new FixedTicker(100, () => {
      ticks += 1
    })
    ticker.update(0.35)
    expect(ticks).toEqual(3)
  })

  it('computes wave rewards deterministically', () => {
    const reward = rewardForEnemy(5, true, 0.1)
    expect(reward.duskenCoin).toBeGreaterThan(0)
    expect(reward.bloodShards).toBe(1)
  })
})


