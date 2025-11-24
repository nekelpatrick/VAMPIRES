## VAMPIRES Monorepo

Short guide to keep everything consistent across the Unity client and Node.js server stack.

### Structure

- `Vampires & Werewolves/`: Unity project (C#) — iOS, Android, WebGL builds.
- `server/`: Fastify backend, PostgreSQL/Redis/S3 ready.
- `client-legacy/`: Archived Three.js + bitecs implementation (for reference only).
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

### Quick Start (Unity Client)

1. Open Unity Hub.
2. Add project: select the `Vampires & Werewolves/` folder.
3. Open with Unity **2022 LTS** (recommended).
4. Open `Assets/Scenes/SampleScene.unity` (or main scene once created).
5. Press **Play** to run in Editor.

Configure the server endpoint in a `GameConfig` ScriptableObject or via environment-based config for builds.

### Conventions

- Always reference the bracketed canon variables as-is (`[THRALL]`, `[DUSKEN COIN]`, etc.).
- Don't over-engineer; land MVP slices per the GDD roadmap.
- Follow Unity C# conventions: PascalCase for public members, camelCase for private.
- Use Prefabs for reusable game objects; ScriptableObjects for data containers.
- Server communication via REST + WebSocket (no tRPC from Unity).
