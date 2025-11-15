import { z } from 'zod'

const envSchema = z.object({
  NODE_ENV: z.enum(['development', 'test', 'production']).default('development'),
  APP_HOST: z.string().min(1).default('0.0.0.0'),
  APP_PORT: z.coerce.number().int().positive().default(3000),
  BASE_URL: z.string().min(1).default('http://localhost:8080'),
  APP_BASE_URL: z.string().min(1).default('http://localhost:8080'),
  IMAGE_BASE_URL: z.string().min(1).default('http://localhost:8080/assets'),
  DB_HOST: z.string().min(1).default('localhost'),
  DB_PORT: z.coerce.number().int().positive().default(5432),
  DB_USER: z.string().min(1).default('postgres'),
  DB_PASSWORD: z.string().min(1).default('postgres'),
  DB_NAME: z.string().min(1).default('vampires'),
  REDIS_HOST: z.string().min(1).default('localhost'),
  REDIS_PORT: z.coerce.number().int().positive().default(6379),
  REDIS_PASSWORD: z.string().optional(),
  S3_ENDPOINT: z.string().min(1).default('http://localhost:9000'),
  S3_BUCKET: z.string().min(1).default('vampires'),
  S3_REGION: z.string().min(1).default('us-east-1'),
  S3_ACCESS_KEY: z.string().min(1).default('minio'),
  S3_SECRET_KEY: z.string().min(1).default('minio123'),
  OPENAI_API_KEY: z.string().min(1).default('changeme'),
  SENDGRID_API_KEY: z.string().min(1).default('changeme'),
  GOOGLE_CLIENT_ID: z.string().min(1).default('changeme'),
  GOOGLE_CLIENT_SECRET: z.string().min(1).default('changeme'),
  GOOGLE_REDIRECT_URL: z.string().min(1).default('http://localhost:3333/oauth/google'),
  LINKEDIN_CLIENT_ID: z.string().optional(),
  LINKEDIN_CLIENT_SECRET: z.string().optional(),
  LINKEDIN_REDIRECT_URL: z.string().optional(),
  JWT_SECRET_KEY: z.string().min(32).default('changemechangemechangemechangeme'),
  RESET_PASSWORD_JWT_SECRET_KEY: z.string().min(32).default('changemechangemechangemechangeme'),
  AI_BASE_URL: z.string().min(1).default('https://api.openai.com'),
  WEBHOOK_URL: z.string().optional(),
  SENDER_EMAIL: z.string().min(1).default('no-reply@vampires.gg'),
  ORGANISATION_NAME: z.string().min(1).default('Vampires Studio'),
  WELCOME_EMAIL_TEMPLATE_ID: z.string().min(1).default('template-welcome'),
  VERIFY_EMAIL_TEMPLATE_ID: z.string().min(1).default('template-verify'),
  RESET_PASSWORD_TEMPLATE_ID: z.string().min(1).default('template-reset'),
  INVITE_EMAIL_TEMPLATE_ID: z.string().min(1).default('template-invite'),
  RESET_PASSWORD_FRONTEND_PAGE: z.string().min(1).default('https://play.vampires.gg/reset'),
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

