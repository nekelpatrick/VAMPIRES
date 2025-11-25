import cors from '@fastify/cors'
import swagger from '@fastify/swagger'
import swaggerUi from '@fastify/swagger-ui'
import { FastifyPluginAsync, FastifyServerOptions } from 'fastify'

import { env } from './config/env'
import type {} from './types/fastify'
import { battleRoutes } from './server/routes/battle'
import { pvpRoutes } from './server/routes/pvp'
import { revivalRoutes } from './server/routes/revival'
import { registerHealthRoutes } from './server/routes/health'
import { registerSpecRoute } from './server/routes/spec'
import { registerTrpcPlugin } from './trpc/plugin'

export type AppOptions = FastifyServerOptions

const options: AppOptions = {}

const app: FastifyPluginAsync<AppOptions> = async (fastify) => {
  await fastify.register(cors, {
    origin: true,
    credentials: true
  })

  fastify.decorate('env', env)

  await fastify.register(swagger, {
    openapi: {
      info: {
        title: 'VAMPIRES Backend API',
        description:
          'Fastify services for [THRALL] combat, [GAIN MONEY] economy, and [ASHEN ONE] monetization layers.',
        version: '0.1.0'
      },
      tags: [
        { name: 'Diagnostics', description: 'Health and readiness endpoints' },
        { name: 'Player', description: 'Player state and progression' },
        { name: 'Thrall', description: '[THRALL] management' },
        { name: 'Economy', description: '[DUSKEN COIN] and [BLOOD SHARDS] balances' },
        { name: 'battle', description: 'Server-authoritative combat resolution' },
        { name: 'pvp', description: 'PvP matchmaking and battles' },
        { name: 'revival', description: '[DEATH] and resurrection system' }
      ],
      servers: [
        { url: env.BASE_URL, description: 'Public Gateway' },
        { url: `http://localhost:${env.APP_PORT}`, description: 'Local' }
      ]
    }
  })

  await fastify.register(swaggerUi, {
    routePrefix: '/docs',
    uiConfig: {
      docExpansion: 'list',
      deepLinking: false
    }
  })

  await fastify.register(registerTrpcPlugin)
  await fastify.register(registerHealthRoutes)
  await fastify.register(registerSpecRoute)
  await fastify.register(battleRoutes)
  await fastify.register(pvpRoutes)
  await fastify.register(revivalRoutes)
}

export default app
export { app, options }
