import { createTRPCClient, httpBatchLink } from '@trpc/client'

import type { AppRouter } from '../../../server/src/trpc/router'

const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:3000'

export const trpcClient = createTRPCClient<AppRouter>({
  links: [
    httpBatchLink({
      url: `${API_BASE}/trpc`,
      fetch: (input, init) =>
        fetch(input, {
          ...init,
          headers: {
            'content-type': 'application/json',
            ...init?.headers
          }
        })
    })
  ]
})

