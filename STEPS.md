## Development Steps

Progress tracker mapped to the GDD roadmap (M0–M6). Marked with `[X]` when completed in this repo.

- [x] **M0 — Infra & Skeleton**  
       Monorepo created, Fastify backend scaffolded with tRPC, env scaffolding, Swagger, and lint/test tooling.
- [x] **M1 — Idle Combat Core (Client Stub)**  
       First Vite-based Three.js/bitecs client slice with `[THRALL]` marching loop, HUD surfacing `[DUSKEN COIN]` and `[BLOOD SHARDS]`, and blood trail VFX.
- [x] **Client ↔ Server Handshake**  
       Integrated browser tRPC client with Fastify `/trpc/player.getProfile`, schema validation, and graceful offline fallback to keep canonical stats synced.
- [ ] **M2 — Deterministic Combat (Workers)**  
      Server battle simulator + replay sync services. _Client-side fixed-tick combat loop, HUD/HP sync, deterministic locomotion, and spawn/camera tuning landed; awaiting server worker implementation._
- [ ] **M3 — PvP + Resurrection**  
       Matchmaker queues, `[DEATH]` handling, revival flows.
- [ ] **M4 — Economy & Monetization**  
       `[DUSKEN COIN]`, `[BLOOD SHARDS]`, `[ASHEN ONE]` subscription + shop.
- [ ] **M5 — Polish**  
       Gore VFX, clan systems, avatar generation pipeline. _New HUD layout, camera framing, and battlefield presentation now mirror reference mobile RPG UI._
- [ ] **M6 — Soft Launch**  
       Analytics, A/B experiments, performance tuning and release prep.

Keep this list updated whenever a milestone flips from `[ ]` to `[X]` so we always know where the solo-dev effort stands.
