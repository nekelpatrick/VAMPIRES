import 'fastify'
import type { Env } from '../config/env'

declare module 'fastify' {
  interface FastifyInstance {
    env: Env
  }
}

