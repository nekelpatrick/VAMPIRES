using UnityEngine;

public class RewardHandler : MonoBehaviour
{
    private CombatManager combatManager;
    private CurrencyManager currencyManager;
    private HordeSpawner hordeSpawner;

    void Start()
    {
        combatManager = CombatManager.Instance;
        currencyManager = CurrencyManager.Instance;
        hordeSpawner = FindFirstObjectByType<HordeSpawner>();

        if (combatManager != null)
        {
            combatManager.OnEnemyDeath += HandleEnemyKill;
        }

        if (hordeSpawner != null)
        {
            hordeSpawner.OnWaveCompleted += HandleWaveComplete;
        }
    }

    void HandleEnemyKill(EnemyController enemy)
    {
        if (currencyManager == null) return;

        EnemyReward reward = RewardCalculator.CalculateReward(
            enemy.WaveNumber,
            enemy.IsElite,
            Random.value
        );

        currencyManager.AddDuskenCoin(reward.duskenCoin);
        currencyManager.AddBloodShards(reward.bloodShards);
    }

    void HandleWaveComplete(int wave)
    {
        if (currencyManager == null) return;

        int bonus = RewardCalculator.WaveCompletionBonus(wave);
        if (bonus > 0)
        {
            currencyManager.AddBloodShards(bonus);
        }
    }

    void OnDestroy()
    {
        if (combatManager != null)
        {
            combatManager.OnEnemyDeath -= HandleEnemyKill;
        }

        if (hordeSpawner != null)
        {
            hordeSpawner.OnWaveCompleted -= HandleWaveComplete;
        }
    }
}

