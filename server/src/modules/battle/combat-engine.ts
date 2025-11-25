export interface CombatStats {
  maxHealth: number
  attack: number
  defense: number
  speed: number
}

export interface CombatEntity {
  id: string
  name: string
  stats: CombatStats
  currentHealth: number
  actionPoints: number
  isAlive: boolean
  team: 'player' | 'enemy'
}

export type BattleEventType =
  | 'BATTLE_START'
  | 'TICK'
  | 'ATTACK'
  | 'DAMAGE'
  | 'DEATH'
  | 'BATTLE_END'

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
  attackerAttack: number,
  targetDefense: number,
  rng: () => number
): number {
  const baseDamage = Math.max(1, attackerAttack - targetDefense * 0.5)
  const variance = 0.2
  const multiplier = 1 + (rng() * 2 - 1) * variance
  return Math.round(baseDamage * multiplier)
}

function selectTarget(
  attacker: CombatEntity,
  entities: CombatEntity[]
): CombatEntity | null {
  const enemies = entities.filter(
    (e) => e.isAlive && e.team !== attacker.team
  )
  if (enemies.length === 0) return null
  return enemies[0]
}

function createEntity(
  id: string,
  name: string,
  stats: CombatStats,
  team: 'player' | 'enemy'
): CombatEntity {
  return {
    id,
    name,
    stats: { ...stats },
    currentHealth: stats.maxHealth,
    actionPoints: 0,
    isAlive: true,
    team
  }
}

export function resolveBattle(
  thrallStats: CombatStats,
  thrallName: string,
  enemyConfigs: Array<{ id: string; name: string; stats: CombatStats }>,
  seed: number,
  maxTicks = 1000
): BattleOutcome {
  const rng = mulberry32(seed)
  const events: BattleEvent[] = []
  let tick = 0

  const thrall = createEntity('thrall', thrallName, thrallStats, 'player')
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
      thrall: { id: thrall.id, name: thrall.name, hp: thrall.currentHealth },
      enemies: enemies.map((e) => ({ id: e.id, name: e.name, hp: e.currentHealth }))
    }
  })

  while (tick < maxTicks) {
    tick++

    for (const entity of allEntities) {
      if (!entity.isAlive) continue
      entity.actionPoints += entity.stats.speed * 10
    }

    const actingEntities = allEntities
      .filter((e) => e.isAlive && e.actionPoints >= 100)
      .sort((a, b) => b.actionPoints - a.actionPoints)

    for (const actor of actingEntities) {
      if (!actor.isAlive) continue

      const target = selectTarget(actor, allEntities)
      if (!target) continue

      actor.actionPoints -= 100

      const damage = computeDamage(actor.stats.attack, target.stats.defense, rng)

      events.push({
        tick,
        type: 'ATTACK',
        actorId: actor.id,
        targetId: target.id
      })

      target.currentHealth -= damage

      events.push({
        tick,
        type: 'DAMAGE',
        actorId: actor.id,
        targetId: target.id,
        value: damage,
        data: { remainingHp: Math.max(0, target.currentHealth) }
      })

      if (actor.team === 'player') {
        damageDealt += damage
      } else {
        damageTaken += damage
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

