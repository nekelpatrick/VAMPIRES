using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public event Action<EnemyController> OnEnemyDeath;
    public event Action OnThrallDied;
    public event Action OnCombatPaused;
    public event Action OnCombatResumed;

    [SerializeField] private float tickIntervalMs = 100f;

    private ThrallController thrall;
    private List<EnemyController> activeEnemies = new List<EnemyController>();
    private float tickAccumulator;
    private float tickInterval;
    private bool combatActive = true;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        tickInterval = tickIntervalMs / 1000f;
    }

    void Update()
    {
        if (!combatActive) return;
        if (thrall == null || !thrall.IsAlive) return;

        tickAccumulator += Time.deltaTime;

        while (tickAccumulator >= tickInterval)
        {
            ProcessTick(tickInterval);
            tickAccumulator -= tickInterval;
        }
    }

    void ProcessTick(float deltaTime)
    {
        if (thrall != null && thrall.IsAlive)
        {
            thrall.AccumulateAP(deltaTime);
            if (thrall.CanAct())
            {
                thrall.ConsumeAP();
                thrall.PerformAction();
            }
        }

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyController enemy = activeEnemies[i];
            if (enemy == null || !enemy.IsAlive) continue;

            enemy.AccumulateAP(deltaTime);
            if (enemy.CanAct())
            {
                enemy.ConsumeAP();
                enemy.PerformAction();
            }
        }
    }

    public void RegisterThrall(ThrallController t)
    {
        thrall = t;
    }

    public void UnregisterThrall(ThrallController t)
    {
        if (thrall == t)
        {
            thrall = null;
        }
    }

    public void RegisterEnemy(EnemyController enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(EnemyController enemy)
    {
        activeEnemies.Remove(enemy);
    }

    public ThrallController GetThrall()
    {
        return thrall;
    }

    public EnemyController GetNearestEnemy(Vector3 position)
    {
        EnemyController nearest = null;
        float minDistance = float.MaxValue;

        foreach (var enemy in activeEnemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;

            float dist = Vector3.Distance(position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }

    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    public void OnEnemyKilled(EnemyController enemy)
    {
        OnEnemyDeath?.Invoke(enemy);
    }

    public void OnThrallDeath()
    {
        combatActive = false;
        OnThrallDied?.Invoke();
    }

    public void PauseCombat()
    {
        combatActive = false;
        OnCombatPaused?.Invoke();
    }

    public void ResumeCombat()
    {
        combatActive = true;
        OnCombatResumed?.Invoke();
    }
}

