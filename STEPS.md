## Development Steps

Progress tracker mapped to the GDD roadmap (M0–M6). Marked with `[X]` when completed in this repo.

### Unity Client Migration

The project has migrated from a JavaScript/Three.js client to **Unity (C#)**. Previous Three.js work is archived in `client-legacy/` for reference.

---

- [x] **M0 — Infra & Skeleton**  
       Unity project setup (URP, folder structure), Fastify server scaffold, CI/CD pipeline, basic scene with side-view camera.

- [x] **M1 — Idle Combat Core**  
       Thrall prefab, Horde spawner, basic AI movement, VFX particles, HUD canvas showing `[DUSKEN COIN]` and `[BLOOD SHARDS]`.

- [ ] **M2 — Deterministic Combat (Node.js)**  
       Server battle simulator (ResolveBattle) + replay sync with Unity client via REST/WebSocket.

- [ ] **M3 — PvP + Resurrection**  
       Matchmaker queues, `[DEATH]` handling, revival flows, Unity UI screens.

- [ ] **M4 — Economy & Monetization**  
       `[DUSKEN COIN]`, `[BLOOD SHARDS]`, `[ASHEN ONE]` subscription + shop UI in Unity.

- [ ] **M5 — Polish**  
       Gore VFX (VFX Graph), deeper abilities, clan systems, avatar generation pipeline.

- [ ] **M6 — Soft Launch**  
       Analytics (Unity Analytics / Firebase), A/B experiments, performance tuning across iOS/Android/WebGL.

---

### Archived Progress (Three.js Client — Legacy)

The following milestones were completed in the previous JavaScript implementation. Code is preserved in `client-legacy/`:

- [x] ~~M0 — Infra & Skeleton (JS)~~  
       Monorepo created, Fastify backend scaffolded with tRPC, env scaffolding, Swagger, and lint/test tooling.

- [x] ~~M1 — Idle Combat Core (JS)~~  
       First Vite-based Three.js/bitecs client slice with `[THRALL]` marching loop, HUD surfacing `[DUSKEN COIN]` and `[BLOOD SHARDS]`, and blood trail VFX.

- [x] ~~Client ↔ Server Handshake (JS)~~  
       Integrated browser tRPC client with Fastify `/trpc/player.getProfile`, schema validation, and graceful offline fallback.

---

Keep this list updated whenever a milestone flips from `[ ]` to `[X]` so we always know where the solo-dev effort stands.
