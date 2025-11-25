-- Migration: 004_economy
-- Description: Player wallets and transaction logging for [DUSKEN COIN] and [BLOOD SHARDS]

CREATE TABLE IF NOT EXISTS wallets (
    player_id TEXT PRIMARY KEY,
    dusken_coin_balance INTEGER NOT NULL DEFAULT 0,
    blood_shard_balance INTEGER NOT NULL DEFAULT 0,
    premium_status TEXT NOT NULL DEFAULT 'NONE',
    premium_expires_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_wallets_premium_status ON wallets(premium_status);
CREATE INDEX IF NOT EXISTS idx_wallets_premium_expires ON wallets(premium_expires_at) WHERE premium_expires_at IS NOT NULL;

CREATE TABLE IF NOT EXISTS transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id TEXT NOT NULL,
    currency TEXT NOT NULL,
    amount INTEGER NOT NULL,
    reason TEXT NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_transactions_player ON transactions(player_id);
CREATE INDEX IF NOT EXISTS idx_transactions_created_at ON transactions(created_at DESC);

COMMENT ON TABLE wallets IS 'Player currency balances and premium subscription status';
COMMENT ON COLUMN wallets.premium_status IS 'NONE or [ASHEN ONE]';
COMMENT ON COLUMN wallets.premium_expires_at IS 'When [ASHEN ONE] subscription expires';

COMMENT ON TABLE transactions IS 'Audit trail for all currency changes';
COMMENT ON COLUMN transactions.currency IS '[DUSKEN COIN] or [BLOOD SHARDS]';
COMMENT ON COLUMN transactions.reason IS 'Source: quest_reward, pvp_reward, battle_reward, revival_cost, upgrade_cost, shop_purchase';

-- Down Migration (run manually if needed)
-- DROP INDEX IF EXISTS idx_transactions_created_at;
-- DROP INDEX IF EXISTS idx_transactions_player;
-- DROP TABLE IF EXISTS transactions;
-- DROP INDEX IF EXISTS idx_wallets_premium_expires;
-- DROP INDEX IF EXISTS idx_wallets_premium_status;
-- DROP TABLE IF EXISTS wallets;

