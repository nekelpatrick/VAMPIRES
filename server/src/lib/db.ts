import { Pool } from 'pg'

import { env } from '../config/env'

let pool: Pool | null = null

export function getPool(): Pool {
  if (pool) {
    return pool
  }

  pool = new Pool({
    host: env.DB_HOST,
    port: env.DB_PORT,
    user: env.DB_USER,
    password: env.DB_PASSWORD,
    database: env.DB_NAME,
    max: 10,
    idleTimeoutMillis: 30000,
    connectionTimeoutMillis: 5000
  })

  pool.on('error', (err) => {
    console.error('Unexpected database error:', err)
  })

  return pool
}

export async function closePool(): Promise<void> {
  if (pool) {
    await pool.end()
    pool = null
  }
}

export async function testConnection(): Promise<boolean> {
  try {
    const p = getPool()
    const result = await p.query('SELECT 1 as test')
    return result.rows[0]?.test === 1
  } catch {
    return false
  }
}

export async function runMigration(sql: string): Promise<void> {
  const p = getPool()
  await p.query(sql)
}

