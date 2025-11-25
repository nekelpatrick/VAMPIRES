-- Migration: 002_pvp
-- Description: Add PvP matchmaking and thrall status tables

-- Thralls table
CREATE TABLE IF NOT EXISTS thralls (
    id TEXT PRIMARY KEY,
    owner_id TEXT NOT NULL,
    archetype TEXT NOT NULL DEFAULT '[WEREWOLF]',
    level INTEGER NOT NULL DEFAULT 1,
    hp INTEGER NOT NULL,
    max_hp INTEGER NOT NULL,
    attack INTEGER NOT NULL,
    defense INTEGER NOT NULL,
    speed REAL NOT NULL DEFAULT 1.0,
    status TEXT NOT NULL DEFAULT 'ACTIVE',
    died_at TIMESTAMPTZ,
    revive_at TIMESTAMPTZ,
    pvp_wins INTEGER NOT NULL DEFAULT 0,
    pvp_losses INTEGER NOT NULL DEFAULT 0,
    death_count INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_thralls_owner ON thralls(owner_id);
CREATE INDEX IF NOT EXISTS idx_thralls_status ON thralls(status);

-- PvP Matches table
CREATE TABLE IF NOT EXISTS pvp_matches (
    id UUID PRIMARY KEY,
    player1_id TEXT NOT NULL,
    player1_thrall_id TEXT NOT NULL,
    player2_id TEXT NOT NULL,
    player2_thrall_id TEXT NOT NULL,
    winner_id TEXT,
    loser_id TEXT,
    battle_id UUID REFERENCES battles(id),
    status TEXT NOT NULL DEFAULT 'PENDING',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_pvp_matches_player1 ON pvp_matches(player1_id);
CREATE INDEX IF NOT EXISTS idx_pvp_matches_player2 ON pvp_matches(player2_id);
CREATE INDEX IF NOT EXISTS idx_pvp_matches_status ON pvp_matches(status);

-- PvP Queue table (for persistent queue if needed)
CREATE TABLE IF NOT EXISTS pvp_queue (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id TEXT NOT NULL UNIQUE,
    thrall_id TEXT NOT NULL,
    power_score REAL NOT NULL,
    joined_at TIMESTAMPTZ DEFAULT NOW(),
    status TEXT NOT NULL DEFAULT 'WAITING'
);

CREATE INDEX IF NOT EXISTS idx_pvp_queue_status ON pvp_queue(status);
CREATE INDEX IF NOT EXISTS idx_pvp_queue_power_score ON pvp_queue(power_score);

