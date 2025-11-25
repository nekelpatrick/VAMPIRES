using UnityEngine;

public static class RewardCalculator
{
    public static EnemyReward CalculateReward(int wave, bool isElite, float roll)
    {
        int baseDusken = 8 + wave * 2;
        int duskenCoin = isElite ? baseDusken * 2 : baseDusken;
        float bloodShardChance = isElite ? 0.3f : 0.05f;
        int bloodShards = roll < bloodShardChance ? 1 : 0;

        return new EnemyReward
        {
            duskenCoin = duskenCoin,
            bloodShards = bloodShards
        };
    }

    public static int WaveCompletionBonus(int wave)
    {
        return wave % 10 == 0 ? 1 : 0;
    }
}

public struct EnemyReward
{
    public int duskenCoin;
    public int bloodShards;
}

