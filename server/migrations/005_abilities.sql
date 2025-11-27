-- Migration: 005_abilities
-- Description: Abilities system for [THRALL] combat powers

CREATE TABLE IF NOT EXISTS abilities (
    id TEXT PRIMARY KEY,
    type TEXT NOT NULL,
    trigger TEXT NOT NULL,
    chance REAL NOT NULL DEFAULT 1.0,
    magnitude REAL NOT NULL,
    duration INTEGER NOT NULL DEFAULT 0,
    cooldown INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_abilities_type ON abilities(type);

CREATE TABLE IF NOT EXISTS thrall_abilities (
    thrall_id TEXT NOT NULL,
    ability_id TEXT NOT NULL,
    unlocked_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (thrall_id, ability_id)
);

CREATE INDEX IF NOT EXISTS idx_thrall_abilities_thrall ON thrall_abilities(thrall_id);

INSERT INTO abilities (id, type, trigger, chance, magnitude, duration, cooldown) VALUES
    ('ability-lifesteal', 'LIFESTEAL', 'ON_ATTACK', 1.0, 0.1, 0, 0),
    ('ability-bleed', 'BLEED', 'ON_HIT', 0.25, 5, 3, 0),
    ('ability-stun', 'STUN', 'ON_HIT', 0.1, 0, 1, 5),
    ('ability-rage', 'RAGE', 'ON_LOW_HEALTH', 1.0, 0.5, 5, 30),
    ('ability-howl', 'HOWL', 'ACTIVE', 1.0, 0.2, 3, 15)
ON CONFLICT (id) DO NOTHING;

COMMENT ON TABLE abilities IS 'Ability definitions per GDD section 12.2';
COMMENT ON COLUMN abilities.type IS 'LIFESTEAL | BLEED | STUN | RAGE | HOWL';
COMMENT ON COLUMN abilities.trigger IS 'ON_ATTACK | ON_HIT | ON_KILL | ON_LOW_HEALTH | ACTIVE';
COMMENT ON COLUMN abilities.magnitude IS 'Effect strength (percentage or flat value depending on type)';
COMMENT ON COLUMN abilities.duration IS 'Effect duration in combat ticks';
COMMENT ON COLUMN abilities.cooldown IS 'Ticks before ability can trigger again';

COMMENT ON TABLE thrall_abilities IS 'Links [THRALL]s to their unlocked abilities';

-- Down Migration (run manually if needed)
-- DROP INDEX IF EXISTS idx_thrall_abilities_thrall;
-- DROP TABLE IF EXISTS thrall_abilities;
-- DROP INDEX IF EXISTS idx_abilities_type;
-- DROP TABLE IF EXISTS abilities;

