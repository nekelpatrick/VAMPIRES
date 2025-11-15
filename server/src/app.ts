import cors from '@fastify/cors'
import { FastifyPluginAsync, FastifyServerOptions } from 'fastify'

import { env } from './config/env'
import type {} from './types/fastify'
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

  await fastify.register(registerTrpcPlugin)
  await fastify.register(registerHealthRoutes)
  await fastify.register(registerSpecRoute)
}

export default app
export { app, options }
