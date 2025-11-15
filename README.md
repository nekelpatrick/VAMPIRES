## VAMPIRES Monorepo

Short guide to keep everything consistent across the client/server stack.

### Structure

- `client/` (coming soon): Three.js + bitecs frontline.
- `server/`: Fastify+tRPC backend, PostgreSQL/Redis/S3 ready.
- `Docs/`: Authoritative GDD plus any specs.
- `VARIABLES.ts`: Canonical `[THRALL]`, `[DUSKEN COIN]`, etc. shared references.

### Quick Start (Server)

```bash
cd server
cp .env.example .env
npm install
npm run dev
```

Swagger UI lives at `http://localhost:<APP_PORT>/docs` (defaults to `3000`).

### Conventions

- Always reference the bracketed canon variables as-is.
- Don’t over-engineer; land MVP slices per the GDD roadmap.
- Fastify CLI scaffold stays intact—extend via modules/plugins instead of rewrites.
