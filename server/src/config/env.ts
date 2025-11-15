import { z } from 'zod'

const envSchema = z.object({
  NODE_ENV: z.enum(['development', 'test', 'production']).default('development'),
  APP_HOST: z.string().min(1).default('0.0.0.0'),
  APP_PORT: z.coerce.number().int().positive().default(3000),
  BASE_URL: z.string().min(1).default('http://localhost:8080'),
  APP_BASE_URL: z.string().min(1).default('http://localhost:8080'),
  IMAGE_BASE_URL: z.string().min(1).default('http://localhost:8080/assets'),
  DB_HOST: z.string().min(1),
  DB_PORT: z.coerce.number().int().positive().default(5432),
  DB_USER: z.string().min(1),
  DB_PASSWORD: z.string().min(1),
  DB_NAME: z.string().min(1),
  REDIS_HOST: z.string().min(1),
  REDIS_PORT: z.coerce.number().int().positive().default(6379),
  REDIS_PASSWORD: z.string().optional(),
  S3_ENDPOINT: z.string().min(1).default('http://localhost:9000'),
  S3_BUCKET: z.string().min(1),
  S3_REGION: z.string().min(1),
  S3_ACCESS_KEY: z.string().min(1),
  S3_SECRET_KEY: z.string().min(1),
  OPENAI_API_KEY: z.string().min(1),
  SENDGRID_API_KEY: z.string().min(1),
  GOOGLE_CLIENT_ID: z.string().min(1),
  GOOGLE_CLIENT_SECRET: z.string().min(1),
  GOOGLE_REDIRECT_URL: z.string().min(1),
  LINKEDIN_CLIENT_ID: z.string().optional(),
  LINKEDIN_CLIENT_SECRET: z.string().optional(),
  LINKEDIN_REDIRECT_URL: z.string().optional(),
  JWT_SECRET_KEY: z.string().min(32),
  RESET_PASSWORD_JWT_SECRET_KEY: z.string().min(32),
  AI_BASE_URL: z.string().min(1).default('https://api.openai.com'),
  WEBHOOK_URL: z.string().optional(),
  SENDER_EMAIL: z.string().min(1),
  ORGANISATION_NAME: z.string().min(1),
  WELCOME_EMAIL_TEMPLATE_ID: z.string().min(1),
  VERIFY_EMAIL_TEMPLATE_ID: z.string().min(1),
  RESET_PASSWORD_TEMPLATE_ID: z.string().min(1),
  INVITE_EMAIL_TEMPLATE_ID: z.string().min(1),
  RESET_PASSWORD_FRONTEND_PAGE: z.string().min(1),
  SUBSCRIPTION_PROVIDER_KEY: z.string().optional()
})

export type Env = z.infer<typeof envSchema>

let cachedEnv: Env | null = null

const loadEnv = (): Env => {
  if (cachedEnv) {
    return cachedEnv
  }

  const parsed = envSchema.safeParse(process.env)

  if (!parsed.success) {
    throw new Error(parsed.error.message)
  }

  cachedEnv = parsed.data
  return cachedEnv
}

export const env = loadEnv()

