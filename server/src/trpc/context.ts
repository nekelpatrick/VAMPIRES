import type { CreateFastifyContextOptions } from '@trpc/server/adapters/fastify'

import { createRequestId } from '../lib/id'

export const createContext = ({ req }: CreateFastifyContextOptions) => {
  return {
    requestId: req.id ?? createRequestId(),
    env: req.server.env
  }
}

export type Context = ReturnType<typeof createContext>

