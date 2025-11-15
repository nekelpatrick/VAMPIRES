# VAMPIRES Web Client

First playable slice of the idle [BATTLEFIELD] built with Vite, Three.js, bitecs, and Rapier (stub).

## Requirements

- Node.js 20+
- Server running on `http://localhost:3000` (override with `VITE_API_BASE`)

## Setup

```bash
cd client
npm install
npm run dev
```

Available scripts:

- `npm run dev` – start Vite dev server
- `npm run build` – type-check and produce a production build
- `npm run preview` – serve the build locally
- `npm run typecheck` – TypeScript no-emit compile
- `npm run lint` / `npm run lint:fix` – ESLint checks
- `npm run format` / `npm run format:fix` – Prettier pass
- `npm run test` – Vitest placeholder suite

## Environment

Set `VITE_API_BASE` to point at the Fastify backend. If unset, the client targets `http://localhost:3000`. The HUD hydrates `[DUSKEN COIN]`, `[BLOOD SHARDS]`, and `[THRALL]` labels via `/trpc/player.getProfile`, falling back to mocked balances if offline.

## Project Layout

```
src/
  ecs/            # bitecs components + systems
  scene/          # Three.js renderer, thrall mesh, particles
  hud/            # DOM overlay for canonical currencies
  services/       # API + Zustand store
  styles/         # global styles
```

