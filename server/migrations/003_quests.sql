-- Migration: 003_quests
-- Description: Daily quest tracking system

CREATE TABLE IF NOT EXISTS daily_quests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id TEXT NOT NULL,
    quest_type TEXT NOT NULL,
    quest_name TEXT NOT NULL,
    description TEXT,
    target_value INT NOT NULL,
    current_value INT DEFAULT 0,
    dusken_reward INT DEFAULT 0,
    blood_shards_reward INT DEFAULT 0,
    completed BOOLEAN DEFAULT FALSE,
    claimed BOOLEAN DEFAULT FALSE,
    ad_bonus_claimed BOOLEAN DEFAULT FALSE,
    quest_date DATE NOT NULL DEFAULT CURRENT_DATE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_daily_quests_player ON daily_quests(player_id);
CREATE INDEX IF NOT EXISTS idx_daily_quests_date ON daily_quests(quest_date);
CREATE INDEX IF NOT EXISTS idx_daily_quests_player_date ON daily_quests(player_id, quest_date);

COMMENT ON TABLE daily_quests IS 'Daily quest progress tracking for player retention';
COMMENT ON COLUMN daily_quests.quest_type IS 'Type: KillEnemies, ReachWave, DealDamage, WinPvP, GetCombo';
COMMENT ON COLUMN daily_quests.ad_bonus_claimed IS 'Whether player watched ad for 2x reward';

-- Down Migration (run manually if needed)
-- DROP INDEX IF EXISTS idx_daily_quests_player_date;
-- DROP INDEX IF EXISTS idx_daily_quests_date;
-- DROP INDEX IF EXISTS idx_daily_quests_player;
-- DROP TABLE IF EXISTS daily_quests;

