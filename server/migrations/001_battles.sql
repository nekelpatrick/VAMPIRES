-- Migration: Create battles table for server-authoritative combat
-- Up Migration

CREATE TABLE IF NOT EXISTS battles (
    id UUID PRIMARY KEY,
    player_id TEXT NOT NULL,
    thrall_id TEXT NOT NULL,
    wave INT NOT NULL,
    seed BIGINT NOT NULL,
    result JSONB NOT NULL,
    events JSONB NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_battles_player ON battles(player_id);
CREATE INDEX IF NOT EXISTS idx_battles_created_at ON battles(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_battles_wave ON battles(wave);

COMMENT ON TABLE battles IS 'Server-authoritative battle records for anti-cheat audit trail';
COMMENT ON COLUMN battles.seed IS 'Deterministic RNG seed for replay verification';
COMMENT ON COLUMN battles.result IS 'Battle outcome: winner, ticks, damage stats';
COMMENT ON COLUMN battles.events IS 'Full event log for client replay';

-- Down Migration (run manually if needed)
-- DROP INDEX IF EXISTS idx_battles_wave;
-- DROP INDEX IF EXISTS idx_battles_created_at;
-- DROP INDEX IF EXISTS idx_battles_player;
-- DROP TABLE IF EXISTS battles;

