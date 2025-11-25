using System;

[Serializable]
public struct CombatStats
{
    public float maxHealth;
    public float attack;
    public float defense;
    public float speed;

    public static CombatStats Scale(CombatStats baseStats, float multiplier)
    {
        return new CombatStats
        {
            maxHealth = baseStats.maxHealth * multiplier,
            attack = baseStats.attack * multiplier,
            defense = baseStats.defense * multiplier,
            speed = baseStats.speed
        };
    }
}

