-- Migration: 006_clans
-- Description: Clan system for [CLAN] membership and bonuses per GDD sections 3.1 and 5.3

CREATE TABLE IF NOT EXISTS clans (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT NOT NULL,
    lifesteal_bonus REAL NOT NULL DEFAULT 0,
    attack_speed_bonus REAL NOT NULL DEFAULT 0,
    bleed_chance_bonus REAL NOT NULL DEFAULT 0,
    curse_chance_bonus REAL NOT NULL DEFAULT 0,
    member_count INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

INSERT INTO clans (id, name, description, lifesteal_bonus, attack_speed_bonus, bleed_chance_bonus, curse_chance_bonus) VALUES
    ('CLAN_NOCTURNUM', 'Clan Nocturnum', 'Ancient royalty. Masters of blood rituals. Passive lifesteal bonuses.', 0.05, 0, 0, 0),
    ('CLAN_SABLEHEART', 'Clan Sableheart', 'Martial aristocrats. Known for brutality and blade mastery. Attack/crit bonuses.', 0, 0.1, 0, 0),
    ('CLAN_ECLIPSA', 'Clan Eclipsa', 'Arcane manipulators and shadowbinders. Status effects, curses, and bleed bonuses.', 0, 0, 0.15, 0.1)
ON CONFLICT (id) DO NOTHING;

CREATE TABLE IF NOT EXISTS player_clan_memberships (
    player_id TEXT PRIMARY KEY,
    clan_id TEXT NOT NULL REFERENCES clans(id),
    joined_at TIMESTAMPTZ DEFAULT NOW(),
    contribution INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_player_clan_clan ON player_clan_memberships(clan_id);
CREATE INDEX IF NOT EXISTS idx_player_clan_contribution ON player_clan_memberships(contribution DESC);

CREATE TABLE IF NOT EXISTS clan_membership_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id TEXT NOT NULL,
    clan_id TEXT NOT NULL,
    action TEXT NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_clan_history_player ON clan_membership_history(player_id);
CREATE INDEX IF NOT EXISTS idx_clan_history_created ON clan_membership_history(created_at DESC);

COMMENT ON TABLE clans IS 'The three vampire [CLAN]s per GDD section 3.1';
COMMENT ON COLUMN clans.lifesteal_bonus IS 'CLAN_NOCTURNUM: +5% lifesteal';
COMMENT ON COLUMN clans.attack_speed_bonus IS 'CLAN_SABLEHEART: +10% attack speed';
COMMENT ON COLUMN clans.bleed_chance_bonus IS 'CLAN_ECLIPSA: +15% bleed application';
COMMENT ON COLUMN clans.curse_chance_bonus IS 'CLAN_ECLIPSA: +10% curse application';

COMMENT ON TABLE player_clan_memberships IS 'Player to [CLAN] assignment';
COMMENT ON COLUMN player_clan_memberships.contribution IS 'PvP wins and achievements contribute to clan standing';

COMMENT ON TABLE clan_membership_history IS 'Audit trail for clan joins/leaves';
COMMENT ON COLUMN clan_membership_history.action IS 'JOIN or LEAVE';

-- Down Migration (run manually if needed)
-- DROP INDEX IF EXISTS idx_clan_history_created;
-- DROP INDEX IF EXISTS idx_clan_history_player;
-- DROP TABLE IF EXISTS clan_membership_history;
-- DROP INDEX IF EXISTS idx_player_clan_contribution;
-- DROP INDEX IF EXISTS idx_player_clan_clan;
-- DROP TABLE IF EXISTS player_clan_memberships;
-- DROP TABLE IF EXISTS clans;

