import { z } from 'zod'

import { battleRouter } from '../modules/battle'
import { economyRouter } from '../modules/economy'
import { playerRouter } from '../modules/player'
import { pvpRouter } from '../modules/pvp'
import { questRouter } from '../modules/quests'
import { shopRouter } from '../modules/shop'
import { subscriptionRouter } from '../modules/subscription'
import { thrallRouter } from '../modules/thrall'
import { CANONICAL_VARIABLES } from '../shared/canonical'
import { procedure, router } from './core'

const healthRouter = router({
  ping: procedure.query(({ ctx }) => ({
    status: 'ok',
    nodeEnv: ctx.env.NODE_ENV
  })),
  canonical: procedure.query(() => ({
    variables: CANONICAL_VARIABLES
  }))
})

const diagnosticsRouter = router({
  echo: procedure
    .input(z.object({ text: z.string().min(1) }))
    .query(({ input }) => ({
      message: input.text,
      timestamp: Date.now()
    }))
})

export const appRouter = router({
  health: healthRouter,
  diagnostics: diagnosticsRouter,
  player: playerRouter,
  thrall: thrallRouter,
  economy: economyRouter,
  battle: battleRouter,
  pvp: pvpRouter,
  quests: questRouter,
  shop: shopRouter,
  subscription: subscriptionRouter
})

export type AppRouter = typeof appRouter
