using System;

[Serializable]
public struct CombatStats
{
    public float maxHealth;
    public float attack;
    public float defense;
    public float speed;
    public float critChance;
    public float lifestealPercent;
    public float bleedChance;

    public static CombatStats Scale(CombatStats baseStats, float multiplier)
    {
        return new CombatStats
        {
            maxHealth = baseStats.maxHealth * multiplier,
            attack = baseStats.attack * multiplier,
            defense = baseStats.defense * multiplier,
            speed = baseStats.speed,
            critChance = baseStats.critChance,
            lifestealPercent = baseStats.lifestealPercent,
            bleedChance = baseStats.bleedChance
        };
    }

    public static CombatStats ApplyClanBonuses(CombatStats stats, ClanType clan)
    {
        var result = stats;
        switch (clan)
        {
            case ClanType.Nocturnum:
                result.lifestealPercent += 0.05f;
                break;
            case ClanType.Sableheart:
                result.speed *= 1.1f;
                break;
            case ClanType.Eclipsa:
                result.bleedChance += 0.15f;
                break;
        }
        return result;
    }
}

public enum ClanType
{
    None = 0,
    Nocturnum = 1,
    Sableheart = 2,
    Eclipsa = 3
}
