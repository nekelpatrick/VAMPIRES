import { FastifyPluginAsync } from 'fastify'

export const registerSpecRoute: FastifyPluginAsync = (fastify) => {
  fastify.get('/swagger', () => {
    return {
      status: 'ready',
      ui: '/docs',
      json: '/docs/json',
      yaml: '/docs/yaml',
      description: 'Fastify-generated OpenAPI spec powering [GAIN MONEY] services'
    }
  })

  return Promise.resolve()
}

