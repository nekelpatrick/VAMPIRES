import awsLambdaFastify from '@fastify/aws-lambda'
import type { APIGatewayProxyEvent, APIGatewayProxyResult, Context } from 'aws-lambda'
import Fastify from 'fastify'

import app from './app'

const fastify = Fastify({
  logger: true
})

fastify.register(app)

const proxy = awsLambdaFastify(fastify, {
  decorateRequest: true
})

let isReady = false

export const handler = async (
  event: APIGatewayProxyEvent,
  context: Context
): Promise<APIGatewayProxyResult> => {
  context.callbackWaitsForEmptyEventLoop = false

  if (!isReady) {
    await fastify.ready()
    isReady = true
  }

  return proxy(event, context)
}

