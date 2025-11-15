# VAMPIRES Backend Skeleton

## Overview

This folder hosts the Fastify + TypeScript backend described in `GAME-DESIGN-DOCUMENT.md`. It focuses on:
- Performance-friendly Fastify HTTP layer with tRPC for shared contracts.
- Canonical references to `[MOBILE]`, `[GAIN MONEY]`, `[THRALL]`, `[DUSKEN COIN]`, `[BLOOD SHARDS]`, `[ASHEN ONE]`, and the other bracketed variables.
- Clean separation between configuration, shared schemas, domain modules, and transport adapters.

## Getting Started

```bash
cd /Users/nekelpatrick/VAMPIRES/server
cp .env.example .env # adjust secrets
npm install
npm run dev
```

Key scripts:
- `npm run dev` – rebuilds on change and restarts Fastify.
- `npm run lint` / `npm run format` – quality gates.
- `npm test` – Node test runner against `/healthz` and `/trpc`.

## Architecture

- `src/config` – environment validation (Zod) and typed Fastify decorations.
- `src/shared` – canonical constants reused across modules.
- `src/modules` – feature domains (`player`, `thrall`, `economy`) exposing schemas, services, and routers.
- `src/trpc` – context, router composition, Fastify plugin.
- `src/server/routes` – transport-specific endpoints such as `/healthz`, `/readyz`, `/swagger`.
- `src/lib` – generic utilities (IDs, future instrumentations).

## Environment & Infra

- `.env.example` enumerates PostgreSQL, Redis, MinIO/S3, OpenAI, SendGrid, Google/LinkedIn OAuth, and monetization keys required by the GDD.
- `docker-compose.yml` spins up Fastify + PostgreSQL + Redis + MinIO for local integration.
- `Dockerfile` builds a production image that runs `node dist/app.js`.

## Next Steps

1. Wire deterministic combat workers and matchmaking queues as independent services once Go workers are ready.
2. Replace stub services with actual persistence adapters (PostgreSQL via Prisma/Drizzle, Redis queues).
3. Expand tRPC routers for resurrection, PvP matchmaking, subscription billing, and `[GAIN MONEY]` telemetry.
# Getting Started with [Fastify-CLI](https://www.npmjs.com/package/fastify-cli)
This project was bootstrapped with Fastify-CLI.

## Available Scripts

In the project directory, you can run:

### `npm run dev`

To start the app in dev mode.\
Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

### `npm start`

For production mode

### `npm run test`

Run the test cases.

## Learn More

To learn Fastify, check out the [Fastify documentation](https://fastify.dev/docs/latest/).
