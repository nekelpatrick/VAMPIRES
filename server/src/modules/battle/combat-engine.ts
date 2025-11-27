import type { Ability, AbilityType, StatusEffect } from '../abilities/ability.schema'
import { ABILITY_DEFINITIONS } from '../abilities/ability.schema'
import type { ClanBonus, ClanType } from '../clan/clan.schema'
import { CLAN_DEFINITIONS } from '../clan/clan.schema'

export interface CombatStats {
  maxHealth: number
  attack: number
  defense: number
  speed: number
  critChance?: number
  lifestealPercent?: number
  bleedChance?: number
}

export function applyClanBonuses(stats: CombatStats, clanId: ClanType | null): CombatStats {
  if (!clanId) return stats

  const clan = CLAN_DEFINITIONS[clanId]
  if (!clan) return stats

  const bonuses = clan.bonuses

  return {
    ...stats,
    speed: stats.speed * (1 + bonuses.attackSpeedBonus),
    lifestealPercent: (stats.lifestealPercent ?? 0) + bonuses.lifestealBonus,
    bleedChance: (stats.bleedChance ?? 0) + bonuses.bleedChanceBonus
  }
}

export interface CombatEntity {
  id: string
  name: string
  stats: CombatStats
  currentHealth: number
  actionPoints: number
  isAlive: boolean
  team: 'player' | 'enemy'
  abilities: Ability[]
  statusEffects: StatusEffect[]
  abilityCooldowns: Map<string, number>
}

export type BattleEventType =
  | 'BATTLE_START'
  | 'TICK'
  | 'ATTACK'
  | 'DAMAGE'
  | 'DEATH'
  | 'BATTLE_END'
  | 'ABILITY_TRIGGER'
  | 'STATUS_APPLIED'
  | 'STATUS_TICK'
  | 'STATUS_EXPIRED'
  | 'HEAL'
  | 'CRITICAL'

export interface BattleEvent {
  tick: number
  type: BattleEventType
  actorId?: string
  targetId?: string
  value?: number
  data?: Record<string, unknown>
}

export interface BattleResult {
  winner: 'player' | 'enemy' | 'draw'
  totalTicks: number
  thrallSurvived: boolean
  enemiesKilled: number
  damageDealt: number
  damageTaken: number
}

export interface BattleOutcome {
  result: BattleResult
  events: BattleEvent[]
  seed: number
}

function mulberry32(seed: number): () => number {
  return () => {
    let t = (seed += 0x6d2b79f5)
    t = Math.imul(t ^ (t >>> 15), t | 1)
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61)
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296
  }
}

function computeDamage(
  attacker: CombatEntity,
  target: CombatEntity,
  rng: () => number
): { damage: number; isCritical: boolean } {
  const critChance = attacker.stats.critChance ?? 0.05
  const isCritical = rng() < critChance
  const critMultiplier = isCritical ? 2.0 : 1.0

  const rageBonus = attacker.statusEffects.some((e) => e.type === 'RAGE') ? 0.5 : 0

  const baseDamage = Math.max(1, attacker.stats.attack - target.stats.defense * 0.5)
  const variance = 0.2
  const multiplier = 1 + (rng() * 2 - 1) * variance

  const finalDamage = Math.round(baseDamage * multiplier * critMultiplier * (1 + rageBonus))

  return { damage: finalDamage, isCritical }
}

function selectTarget(
  attacker: CombatEntity,
  entities: CombatEntity[]
): CombatEntity | null {
  const enemies = entities.filter(
    (e) => e.isAlive && e.team !== attacker.team && !isStunned(e)
  )
  if (enemies.length === 0) {
    return entities.find((e) => e.isAlive && e.team !== attacker.team) ?? null
  }
  return enemies[0]
}

function isStunned(entity: CombatEntity): boolean {
  return entity.statusEffects.some((e) => e.type === 'STUN' && e.remainingDuration > 0)
}

function createEntity(
  id: string,
  name: string,
  stats: CombatStats,
  team: 'player' | 'enemy',
  abilities: Ability[] = []
): CombatEntity {
  return {
    id,
    name,
    stats: { ...stats },
    currentHealth: stats.maxHealth,
    actionPoints: 0,
    isAlive: true,
    team,
    abilities,
    statusEffects: [],
    abilityCooldowns: new Map()
  }
}

function tryTriggerAbilities(
  actor: CombatEntity,
  target: CombatEntity,
  trigger: 'ON_ATTACK' | 'ON_HIT' | 'ON_KILL' | 'ON_LOW_HEALTH',
  tick: number,
  events: BattleEvent[],
  rng: () => number
): void {
  for (const ability of actor.abilities) {
    if (ability.trigger !== trigger) continue

    const cooldownKey = ability.id
    const lastUsed = actor.abilityCooldowns.get(cooldownKey) ?? -Infinity
    if (tick - lastUsed < ability.cooldown) continue

    if (rng() > ability.chance) continue

    actor.abilityCooldowns.set(cooldownKey, tick)

    events.push({
      tick,
      type: 'ABILITY_TRIGGER',
      actorId: actor.id,
      targetId: target.id,
      data: { abilityType: ability.type }
    })

    applyAbilityEffect(actor, target, ability, tick, events)
  }
}

function applyAbilityEffect(
  actor: CombatEntity,
  target: CombatEntity,
  ability: Ability,
  tick: number,
  events: BattleEvent[]
): void {
  switch (ability.type) {
    case 'BLEED': {
      const effect: StatusEffect = {
        id: `bleed-${tick}-${actor.id}`,
        type: 'BLEED',
        sourceId: actor.id,
        targetId: target.id,
        magnitude: ability.magnitude,
        remainingDuration: ability.duration,
        ticksApplied: 0
      }
      target.statusEffects.push(effect)
      events.push({
        tick,
        type: 'STATUS_APPLIED',
        actorId: actor.id,
        targetId: target.id,
        data: { effectType: 'BLEED', duration: ability.duration }
      })
      break
    }

    case 'STUN': {
      const effect: StatusEffect = {
        id: `stun-${tick}-${actor.id}`,
        type: 'STUN',
        sourceId: actor.id,
        targetId: target.id,
        magnitude: 0,
        remainingDuration: ability.duration,
        ticksApplied: 0
      }
      target.statusEffects.push(effect)
      events.push({
        tick,
        type: 'STATUS_APPLIED',
        actorId: actor.id,
        targetId: target.id,
        data: { effectType: 'STUN', duration: ability.duration }
      })
      break
    }

    case 'RAGE': {
      const effect: StatusEffect = {
        id: `rage-${tick}-${actor.id}`,
        type: 'RAGE',
        sourceId: actor.id,
        targetId: actor.id,
        magnitude: ability.magnitude,
        remainingDuration: ability.duration,
        ticksApplied: 0
      }
      actor.statusEffects.push(effect)
      events.push({
        tick,
        type: 'STATUS_APPLIED',
        actorId: actor.id,
        targetId: actor.id,
        data: { effectType: 'RAGE', duration: ability.duration }
      })
      break
    }

    case 'HOWL': {
      events.push({
        tick,
        type: 'STATUS_APPLIED',
        actorId: actor.id,
        data: { effectType: 'HOWL', radius: 5, debuffAmount: ability.magnitude }
      })
      break
    }
  }
}

function processStatusEffects(
  entities: CombatEntity[],
  tick: number,
  events: BattleEvent[]
): void {
  for (const entity of entities) {
    if (!entity.isAlive) continue

    for (let i = entity.statusEffects.length - 1; i >= 0; i--) {
      const effect = entity.statusEffects[i]

      if (effect.type === 'BLEED' && effect.remainingDuration > 0) {
        const bleedDamage = Math.round(effect.magnitude)
        entity.currentHealth -= bleedDamage

        events.push({
          tick,
          type: 'STATUS_TICK',
          targetId: entity.id,
          value: bleedDamage,
          data: { effectType: 'BLEED' }
        })

        if (entity.currentHealth <= 0) {
          entity.isAlive = false
          entity.currentHealth = 0
          events.push({
            tick,
            type: 'DEATH',
            actorId: entity.id,
            data: { killedBy: effect.sourceId, cause: 'BLEED' }
          })
        }
      }

      effect.remainingDuration--
      effect.ticksApplied++

      if (effect.remainingDuration <= 0) {
        entity.statusEffects.splice(i, 1)
        events.push({
          tick,
          type: 'STATUS_EXPIRED',
          targetId: entity.id,
          data: { effectType: effect.type }
        })
      }
    }
  }
}

function applyLifesteal(
  attacker: CombatEntity,
  damage: number,
  tick: number,
  events: BattleEvent[]
): void {
  const lifestealPercent = attacker.stats.lifestealPercent ?? 0
  const hasLifestealAbility = attacker.abilities.some((a) => a.type === 'LIFESTEAL')

  const totalLifesteal = hasLifestealAbility
    ? lifestealPercent + ABILITY_DEFINITIONS.LIFESTEAL.magnitude
    : lifestealPercent

  if (totalLifesteal <= 0) return

  const healAmount = Math.round(damage * totalLifesteal)
  if (healAmount <= 0) return

  attacker.currentHealth = Math.min(
    attacker.stats.maxHealth,
    attacker.currentHealth + healAmount
  )

  events.push({
    tick,
    type: 'HEAL',
    actorId: attacker.id,
    value: healAmount,
    data: { source: 'LIFESTEAL' }
  })
}

function checkLowHealthTriggers(
  entity: CombatEntity,
  tick: number,
  events: BattleEvent[],
  rng: () => number
): void {
  const healthPercent = entity.currentHealth / entity.stats.maxHealth
  if (healthPercent > 0.3) return

  for (const ability of entity.abilities) {
    if (ability.trigger !== 'ON_LOW_HEALTH') continue

    const cooldownKey = ability.id
    const lastUsed = entity.abilityCooldowns.get(cooldownKey) ?? -Infinity
    if (tick - lastUsed < ability.cooldown) continue

    if (rng() > ability.chance) continue

    entity.abilityCooldowns.set(cooldownKey, tick)

    events.push({
      tick,
      type: 'ABILITY_TRIGGER',
      actorId: entity.id,
      data: { abilityType: ability.type }
    })

    applyAbilityEffect(entity, entity, ability, tick, events)
  }
}

export function resolveBattle(
  thrallStats: CombatStats,
  thrallName: string,
  enemyConfigs: Array<{ id: string; name: string; stats: CombatStats }>,
  seed: number,
  maxTicks = 1000,
  thrallAbilities: Ability[] = [],
  thrallClan: ClanType | null = null
): BattleOutcome {
  const rng = mulberry32(seed)
  const events: BattleEvent[] = []
  let tick = 0

  const finalStats = applyClanBonuses(thrallStats, thrallClan)

  const thrall = createEntity('thrall', thrallName, finalStats, 'player', thrallAbilities)
  const enemies = enemyConfigs.map((cfg) =>
    createEntity(cfg.id, cfg.name, cfg.stats, 'enemy')
  )
  const allEntities = [thrall, ...enemies]

  let damageDealt = 0
  let damageTaken = 0
  let enemiesKilled = 0

  events.push({
    tick: 0,
    type: 'BATTLE_START',
    data: {
      thrall: {
        id: thrall.id,
        name: thrall.name,
        hp: thrall.currentHealth,
        abilities: thrall.abilities.map((a) => a.type)
      },
      enemies: enemies.map((e) => ({ id: e.id, name: e.name, hp: e.currentHealth }))
    }
  })

  while (tick < maxTicks) {
    tick++

    processStatusEffects(allEntities, tick, events)

    for (const entity of allEntities) {
      if (!entity.isAlive) continue
      if (isStunned(entity)) continue
      entity.actionPoints += entity.stats.speed * 10
    }

    for (const entity of allEntities) {
      if (entity.isAlive) {
        checkLowHealthTriggers(entity, tick, events, rng)
      }
    }

    const actingEntities = allEntities
      .filter((e) => e.isAlive && e.actionPoints >= 100 && !isStunned(e))
      .sort((a, b) => b.actionPoints - a.actionPoints)

    for (const actor of actingEntities) {
      if (!actor.isAlive) continue

      const target = selectTarget(actor, allEntities)
      if (!target) continue

      actor.actionPoints -= 100

      tryTriggerAbilities(actor, target, 'ON_ATTACK', tick, events, rng)

      const { damage, isCritical } = computeDamage(actor, target, rng)

      events.push({
        tick,
        type: 'ATTACK',
        actorId: actor.id,
        targetId: target.id
      })

      if (isCritical) {
        events.push({
          tick,
          type: 'CRITICAL',
          actorId: actor.id,
          targetId: target.id,
          value: damage
        })
      }

      target.currentHealth -= damage

      events.push({
        tick,
        type: 'DAMAGE',
        actorId: actor.id,
        targetId: target.id,
        value: damage,
        data: { remainingHp: Math.max(0, target.currentHealth), isCritical }
      })

      if (actor.team === 'player') {
        damageDealt += damage
        applyLifesteal(actor, damage, tick, events)
      } else {
        damageTaken += damage
      }

      tryTriggerAbilities(actor, target, 'ON_HIT', tick, events, rng)

      const bleedChance = actor.stats.bleedChance ?? 0
      if (bleedChance > 0 && rng() < bleedChance) {
        const bleedAbility = ABILITY_DEFINITIONS.BLEED
        applyAbilityEffect(
          actor,
          target,
          { ...bleedAbility, id: `passive-bleed-${tick}` },
          tick,
          events
        )
      }

      if (target.currentHealth <= 0) {
        target.isAlive = false
        target.currentHealth = 0

        events.push({
          tick,
          type: 'DEATH',
          actorId: target.id,
          data: { killedBy: actor.id }
        })

        if (target.team === 'enemy') {
          enemiesKilled++
          tryTriggerAbilities(actor, target, 'ON_KILL', tick, events, rng)
        }
      }
    }

    const playerAlive = allEntities.some((e) => e.team === 'player' && e.isAlive)
    const enemyAlive = allEntities.some((e) => e.team === 'enemy' && e.isAlive)

    if (!playerAlive || !enemyAlive) {
      break
    }
  }

  const playerAlive = thrall.isAlive
  const enemyAlive = enemies.some((e) => e.isAlive)

  let winner: 'player' | 'enemy' | 'draw'
  if (playerAlive && !enemyAlive) {
    winner = 'player'
  } else if (!playerAlive && enemyAlive) {
    winner = 'enemy'
  } else {
    winner = 'draw'
  }

  const result: BattleResult = {
    winner,
    totalTicks: tick,
    thrallSurvived: playerAlive,
    enemiesKilled,
    damageDealt,
    damageTaken
  }

  events.push({
    tick,
    type: 'BATTLE_END',
    data: { result }
  })

  return {
    result,
    events,
    seed
  }
}

export function generateEnemiesForWave(
  wave: number,
  baseStats: CombatStats,
  rng: () => number
): Array<{ id: string; name: string; stats: CombatStats }> {
  const enemyCount = Math.min(10, 3 + Math.floor(wave / 2))
  const statMultiplier = 1 + wave * 0.15

  const enemies: Array<{ id: string; name: string; stats: CombatStats }> = []

  for (let i = 0; i < enemyCount; i++) {
    const isElite = rng() < 0.1 + wave * 0.02
    const eliteMultiplier = isElite ? 1.5 : 1

    enemies.push({
      id: `enemy-${wave}-${i}`,
      name: isElite ? 'Elite Horde Minion' : 'Horde Minion',
      stats: {
        maxHealth: Math.round(baseStats.maxHealth * statMultiplier * eliteMultiplier),
        attack: Math.round(baseStats.attack * statMultiplier * eliteMultiplier),
        defense: Math.round(baseStats.defense * statMultiplier * eliteMultiplier),
        speed: baseStats.speed
      }
    })
  }

  return enemies
}

export function createSeed(): number {
  return Math.floor(Math.random() * 2147483647)
}
