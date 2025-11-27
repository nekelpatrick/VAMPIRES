using UnityEngine;

public static class CombatMath
{
    public static int ComputeDamage(float attack, float defense)
    {
        float mitigated = attack - defense * 0.5f;
        return Mathf.Max(1, Mathf.FloorToInt(mitigated));
    }

    public static int ApplyVariance(int baseDamage, float variance, float roll)
    {
        float spread = baseDamage * variance;
        return Mathf.Max(1, Mathf.FloorToInt(baseDamage + (roll - 0.5f) * spread));
    }
}

