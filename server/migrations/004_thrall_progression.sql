-- Migration 004: Add XP column for thrall progression

-- Add xp column to thralls table
ALTER TABLE thralls ADD COLUMN IF NOT EXISTS xp INTEGER NOT NULL DEFAULT 0;

-- Create index for level lookups (useful for matchmaking)
CREATE INDEX IF NOT EXISTS idx_thralls_level ON thralls (level);



