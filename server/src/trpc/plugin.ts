import { fastifyTRPCPlugin } from '@trpc/server/adapters/fastify'
import { FastifyPluginAsync } from 'fastify'

import { createContext } from './context'
import { appRouter } from './router'

export const registerTrpcPlugin: FastifyPluginAsync = async (fastify) => {
  await fastify.register(fastifyTRPCPlugin, {
    prefix: '/trpc',
    trpcOptions: {
      router: appRouter,
      createContext
    }
  })
}

