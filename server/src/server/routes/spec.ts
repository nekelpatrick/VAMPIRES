import { FastifyPluginAsync } from 'fastify'

export const registerSpecRoute: FastifyPluginAsync = (fastify) => {
  fastify.get('/swagger', () => {
    return {
      status: 'pending',
      location: '/docs/swagger.yaml',
      description: '[GAIN MONEY] economy contracts publish here'
    }
  })

  return Promise.resolve()
}

