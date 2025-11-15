Below is the **fully rebuilt**, **complete**, and **detailed** Game Design Document, fully aligned with:

- Your final tech stack
  **→ Client: Open Web Platform (Three.js + Rapier + bitecs ECS)**
  **→ Backend: Fastify (Node.js) with PostgreSQL + Redis + S3/MinIO**
- Your canonical variables:
  `[MOBILE]`, `[GAIN MONEY]`, `[THRALL]`, `[PLAYER]`, `[VAMPIRE]`, `[WEREWOLF]`, `[HORDE]`, `[CLAN]`,
  `[BATTLEFIELD]`, `[NPC]`, `[DEATH]`, `[STRONGER]`, `[TRHALL]`, `[DUSKEN COIN]`,
  `[BLOOD SHARDS]`, `[PREMIUM]`, `[ASHEN ONE]`
- Your creative vision (darkest-dungeon style, gore, 3D side-view, idle RPG)
- Your monetization goals
- Multi-player PvE + PvP
- A professional, production-ready, senior-engineer-grade design

This is your **authoritative full GDD v1**.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# **VAMPIRES AND WEREWOLVES — THE NINE CIRCLES**

### **Full Game Design Document (Web + Mobile)**

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 1. EXECUTIVE SUMMARY

**Vampires and Werewolves — The Nine Circles** is a **[MOBILE] web-first idle RPG** where the [PLAYER]—a newly awakened [VAMPIRE]—commands autonomous [THRALL] units (starting with a [WEREWOLF]) who fight endless [HORDE] waves inside a 3D side-view battlefield.
Combat is **automatic**, **continuous**, and **spectacle-driven**, with gore-heavy animations that make the idle loop visually **[MESMERIZING]** and **[ADDICTIVE]**.

Players collect **[DUSKEN COIN]** (soft currency) and **[BLOOD SHARDS]** (premium currency), upgrade their [THRALL]s, enter PvP duels, survive [DEATH], and participate in clan-based seasonal conflicts.

Monetization centers on purchases, cosmetics, revives, PvP convenience, and an ongoing subscription tier called **[ASHEN ONE]**.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 2. GAME PILLARS

1. **Idle Brutality**

   - Visually stunning, bloody, stylized side-view auto-combat.

2. **Thrall Leadership Fantasy**

   - The [PLAYER] is a master [VAMPIRE] commanding loyal [THRALL]s.

3. **Progression Through Blood**

   - Powerful stat upgrades, gory evolutions, mutations, and clan-aligned bonuses.

4. **Risk and Death**

   - PvP with real danger. If your [THRALL] falls into [DEATH], you must revive it through resources or time.

5. **Persistent Online World**

   - Shared [BATTLEFIELD] instances populated by [NPC]s and other [PLAYER]s.

6. **Monetization Without Shame**

   - A premium layer built for value and identity: **[ASHEN ONE]** subscription, avatar generation, cosmetics, and VIP perks.

7. **Mobile-First Web Technology**

   - Smooth, performant WebGL-based 3D in Three.js with ECS logic and server authoritative combat.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 3. STORY & SETTING

## 3.1 Overview

The [PLAYER], a dormant [VAMPIRE], awakens from centuries of **[VAMPIRE SLUMBER]** in the middle of a brutal **[CIVIL WAR]** between three great [CLAN]s:

### The Three Vampire [CLAN]s (canonical names)

1. **CLAN NOCTURNUM**
   Ancient royalty. Masters of blood rituals. Passive lifesteal bonuses.
2. **CLAN SABLEHEART**
   Martial aristocrats. Known for brutality and blade mastery. Attack/crit bonuses.
3. **CLAN ECLIPSA**
   Arcane manipulators and shadowbinders. Status effects, curses, and bleed bonuses.

## 3.2 The Lackey (Onboarding Guide)

The Lackey is the first speaking character. He:

- explains the [VAMPIRE]’s awakening
- introduces the [CIVIL WAR]
- gives the first [THRALL] (a young [WEREWOLF])
- walks the player into recruiting, upgrades, currencies, PvP risk, and resurrection systems

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 4. GAMEPLAY SYSTEMS (CORE)

## 4.1 The [THRALL]

The [THRALL] is the player’s main unit.

- Always 3D, animated, with several attack patterns.
- Auto-attacks enemies via server-authoritative combat.
- Has: HP, Attack, Defense, Speed, Crit, Resistances, Abilities.
- Can evolve or mutate into stronger forms as levels increase.
- Displays a **HP bar (above)** and **Name Bar (below)**.

The starter [THRALL] is always the **[WEREWOLF]**.

## 4.2 The [BATTLEFIELD]

- A 3D side-view arena using Three.js.
- Infinite waves of enemies.
- Instanced using ECS to keep performance stable.
- Visuals: rotating layers of fog, silhouettes, embers, blood particle systems.
- Players will see many [NPC]s and other [PLAYER]s’ thralls marching beside them.

## 4.3 [HORDE] AI (ENEMIES)

- Procedurally spawning enemies of varying archetypes.
- Horde composition changes as time increases.
- Elite enemies every X waves.
- Boss waves (big rewards).

All enemies are 3D low-poly models that match the darkest-dungeon style.

## 4.4 PvP Mode

Player chooses to send their [THRALL] to PvP:

- Matched against other [PLAYER] thralls.
- Possible [DEATH] is a core risk.
- Outcomes resolved server-side with deterministic simulation.
- Players cannot know if they are facing a [STRONGER] opponent.
- Rewards: more [DUSKEN COIN], some [BLOOD SHARDS], and ranking points.

## 4.5 [DEATH] & Resurrection System

If the [THRALL] dies:

- It becomes unusable until revived.
- Revival methods:

  1. Free revive (long timer)
  2. Pay with **[DUSKEN COIN]** (cheaper but slower)
  3. Pay with **[BLOOD SHARDS]** (instant)
  4. [ASHEN ONE] discounts

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 5. PROGRESSION SYSTEMS

## 5.1 XP & Levels

- Thrall gains XP per wave.
- Level ups increase primary stats.
- Every 10 levels: special ability unlock or evolution.

## 5.2 Gear System (Phase 2 / optional MVP)

- Weapons/armor with rarity tiers.
- Cosmetic differences applied via shaders and color grading.
- Dropped by elite/boss waves or purchased with [BLOOD SHARDS].

## 5.3 Clan Progression

Each [CLAN] gives passive global bonuses:

- Nocturnum: % lifesteal
- Sableheart: % attack speed
- Eclipsa: % bleed/curse application

Players can join a [CLAN] after tutorial.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 6. ECONOMY DESIGN

## 6.1 Soft Currency: **[DUSKEN COIN]**

- Earned constantly from PvE, daily login, quests.
- Used for: thrall upgrades, minor revival, crafting, and low-tier items.
- High-flow currency; easy to spend.

## 6.2 Premium Currency: **[BLOOD SHARDS]**

- Rare currency.
- Purchased with real money or earned minimally through big milestones.
- Used for: instant revive, premium thrall recruitment, cosmetics, clan prestige items.

## 6.3 Subscription Tier: **[ASHEN ONE]**

Equivalent to [PREMIUM].

Subscribers receive:

- Monthly allotment of [BLOOD SHARDS]
- Faster offline accumulation
- Extra loot drop chance
- Priority PvP queue
- Exclusive cosmetics
- Access to AI Avatar Generation
- Badge beside username

Revenue driver for **[GAIN MONEY]**.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 7. USER FLOW (FIRST 5 MINUTES)

1. Player opens game → Play instantly as Guest or Login.
2. Intro cinematic: [VAMPIRE SLUMBER] awakening.
3. Lackey greets and gives the first [WEREWOLF] [THRALL].
4. Spawned directly into [BATTLEFIELD].
5. Thrall auto-attacks weak enemies.
6. Player collects first [DUSKEN COIN].
7. Tutorial prompts upgrade.
8. Offer optional trial for **[ASHEN ONE]**.
9. Player reaches wave 10 → unlocks PvP → sees resurrection warning.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 8. VISUAL STYLE & AUDIO

### Visual Style

- Dark palette: reds, blacks, browns, grey, yellow highlights
- Contrast-heavy shading
- High clarity silhouettes
- Gore particles (blood arcs, splashes)
- VFX intensity scales with device performance

### Audio

- Low-frequency drones
- Sharp hit sounds
- Deep roars, vampire whispers, howls
- Gore squelches, blade slices

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 9. TECHNICAL DESIGN (WEB CLIENT)

### 9.1 Core Technologies

- **Three.js** → rendering
- **bitecs** → ECS for performant entity simulation
- **Rapier** → physics (cosmetic only)
- **TypeScript** → typed game logic
- **Web Workers** → background simulation (optional)
- **IndexedDB** → local caching

### 9.2 Core Systems

- **ECS Entities:** Thrall, Enemy, Boss, VFX particle, DamageNumber
- **Systems:** Movement, Attack, AnimationSync, VFX, HPBarRendering
- **Physics:** Rapier world used for ragdolls / cosmetic knockbacks

### 9.3 Rendering Optimizations

- Instanced meshes for hordes
- GPU particles
- Object pooling
- Frustum culling (side-view simplifies this)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 10. TECHNICAL DESIGN (NODE BACKEND)

Fastify is one of the most solid, production-hardened choices for a Node.js game backend today: it is actively maintained by contributors who also support Node core, is battle-tested by companies like NearForm, Microsoft, and Platformatic, and offers frequent patch releases with a stable API surface. Its lightweight, schema-driven foundation sustains top-tier throughput without hiding the routing, lifecycle, or plugin controls engineers expect. The native TypeScript definitions and existing tRPC adapters let us share types with the Three.js client cleanly, avoiding custom glue while staying lighter than decorator-heavy frameworks such as NestJS. Operational reliability features—JSON schema validation, logging hooks, graceful shutdown, HTTP/2/WebSocket support, and plugin isolation—make it straightforward to run behind load balancers, capture observability, and execute zero-downtime deploys. In short, Fastify provides a dependable, well-supported base that only grows more complex when we opt in, aligning with our requirement for a solid backend without unnecessary bells and whistles.

### 10.1 Services

- **API Gateway (Fastify)**: REST + WebSocket
- **Combat Worker (Node.js)**: deterministic battle simulator
- **Matchmaker (Node.js)**: PvP queue management
- **Avatar Service (Node.js)**: handles avatar generation requests
- **Economy Service (Node.js)**: currency operations, transactions, logs

### 10.2 Persistence

- PostgreSQL → players, thralls, transactions
- Redis → matchmaking queues, caches
- S3/MinIO → user avatars

### 10.3 Battle Simulation (Deterministic)

- Seeded RNG
- Tick-based engine
- Event logs
- Final summary stored and returned to client

### 10.4 Networking

- REST for basic CRUD
- WebSocket for push notifications (battle ready, PvP result)
- All state changes validated server-side

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 11. DATA MODELS (SIMPLIFIED)

### Players

- id, user_id, clan_id, dusken_balance, blood_shards_balance…

### Thralls

- id, owner_id, type, stats, abilities, status, last_revive_at

### Battles

- id, seed, participants, logs, result

### Transactions

- id, player_id, amount, currency, type, reason

### Subscriptions

- id, user_id, active_until, avatar_credits

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 12. COMBAT DESIGN

### 12.1 Tick-Based Model

Each tick (e.g., every 100ms):

- Entities gain AP (action points) based on speed
- When AP >= 100 → perform action
- Choose target deterministically
- Damage Formula:

```
damage = max(1, attacker.attack - target.defense * 0.5)
```

### 12.2 Abilities

- Lifesteal
- Bleed
- Stun
- Rage Mode
- Howl (AoE debuff)

### 12.3 PvP

- PowerScore hidden from player → only “Low / Moderate / High Threat” indicator
- True values hidden to support “Pokémon-like” uncertainty
- Results can cause [DEATH]

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 13. GAME LOOP (MASTER LOOP)

1. Log in
2. Auto-fight on [BATTLEFIELD]
3. Collect [DUSKEN COIN]
4. Upgrade [THRALL]
5. Enter PvP
6. Risk [DEATH]
7. Revive
8. Recruit more thralls (later phases)
9. Progress clan standings
10. Participate in events
11. Spend [BLOOD SHARDS]
12. Repeat (idle automation)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 14. MONETIZATION DESIGN

### 14.1 Core Revenue Streams

1. **[BLOOD SHARDS]** packs
2. **[ASHEN ONE]** monthly subscription
3. Cosmetics: skins, colors
4. AI-generated portrait (subscriber-only)
5. Revives & convenience features
6. Battle Pass (seasonal)

### 14.2 Monetization Ethics

- No “forced loss” loops
- PvP fairness preserved (no direct stat purchases)
- Purchases accelerate, don’t override skill/progression

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 15. MVP SCOPE

### MVP Includes:

- 1 Thrall type: [WEREWOLF]
- Base [BATTLEFIELD]
- Horde enemies (5 variants)
- PvP
- Resurrection
- Soft/Premium currency
- Subscription basics
- Avatar generation (stub)

### MVP Excludes:

- Multi-thrall management
- Crafting
- Clan Wars
- Seasonal events

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 16. ROADMAP / MILESTONES (WEB + GO)

### M0 — Infra & Skeleton

Monorepo, CI/CD, initial ECS, empty Three.js scene, basic Fastify server.

### M1 — Idle Combat Core

Thrall model, Horde spawn, ECS logic, particles, server-logged combat.

### M2 — Deterministic Combat (Node.js)

ResolveBattle implementation + replay sync.

### M3 — PvP + Resurrection

Matchmaker, PvP queue, death state, revival mechanics.

### M4 — Economy & Monetization

[DUSKEN COIN], [BLOOD SHARDS], subscription tier, shop.

### M5 — Polish

Gore VFX, deeper abilities, clan system, avatar generation.

### M6 — Soft Launch

Analytics, A/B testing for monetization, performance tuning.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 17. QA PLAN

- Deterministic combat tests (seeded simulation)
- Load tests for matchmaking
- Mobile performance tests (mid/low devices)
- Network resilience tests
- Currency transaction integrity tests
- “Death Loop” tests for PvP fairness
- Gore slider tests for store compliance

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 18. RISKS & MITIGATION

| Risk                     | Mitigation                                               |
| ------------------------ | -------------------------------------------------------- |
| Low FPS on mobile        | ECS optimization, instancing, LOD, gore intensity slider |
| Perceived pay-to-win     | Cosmetic-first monetization; limit stat differences      |
| Server combat cost       | Worker autoscaling, Redis queue batching                 |
| Large bundle size        | Dynamic imports, model compression, asset streaming      |
| Abusive PvP exploitation | Matchmaking variance + cooldowns                         |

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

# 19. IMMEDIATE DEVELOPMENT ACTIONS (TO START THE PROJECT)

1. **Create monorepo** with `client/` and `server/` folders + CI pipeline.
2. **Implement Node combat worker prototype** with deterministic RNG + event logs.
3. **Create first Three.js scene**: side-view camera + basic thrall mesh + ECS movement + HUD showing [DUSKEN COIN] & [BLOOD SHARDS].

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
