using System;
using System.Collections.Generic;
using UnityEngine;

public class HordeSpawner : MonoBehaviour
{
    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;

    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnXMin = 6f;
    [SerializeField] private float spawnXMax = 10f;
    [SerializeField] private float spawnYVariance = 1.5f;
    [SerializeField] private float eliteChance = 0.1f;

    private CombatManager combatManager;
    private List<EnemyController> pooledEnemies = new List<EnemyController>();

    private int currentWave;
    private int enemiesRemaining;
    private int enemiesToSpawn;
    private int maxConcurrent;
    private float spawnInterval;
    private float spawnTimer;
    private bool waveActive;

    void Awake()
    {
        CreateDefaultEnemyData();
        PrewarmPool(20);
    }

    void Start()
    {
        combatManager = CombatManager.Instance;
        if (combatManager != null)
        {
            combatManager.OnEnemyDeath += HandleEnemyDeath;
        }

        if (spawnPoint == null)
        {
            GameObject sp = GameObject.Find("SpawnPoint");
            if (sp != null) spawnPoint = sp.transform;
        }

        StartWave(1);
    }

    void PrewarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject enemyObj = new GameObject("Enemy_" + i);
            enemyObj.transform.SetParent(transform);
            enemyObj.transform.localScale = Vector3.one * 0.4f;
            enemyObj.SetActive(false);

            EnemyController controller = enemyObj.AddComponent<EnemyController>();
            enemyObj.AddComponent<AttackAnimator>();

            pooledEnemies.Add(controller);
        }
    }

    void CreateDefaultEnemyData()
    {
        if (enemyData == null)
        {
            enemyData = ScriptableObject.CreateInstance<EnemyData>();
            enemyData.enemyName = "Horde Minion";
            enemyData.baseStats = new CombatStats
            {
                maxHealth = 90f,
                attack = 14f,
                defense = 4f,
                speed = 1f
            };
            enemyData.normalColor = new Color(0.5f, 0.6f, 0.4f);
            enemyData.eliteColor = new Color(0.8f, 0.2f, 0.3f);
        }
    }

    void Update()
    {
        if (!waveActive) return;
        if (enemiesToSpawn <= 0) return;
        if (combatManager == null || combatManager.GetActiveEnemyCount() >= maxConcurrent) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    public void StartWave(int wave)
    {
        currentWave = wave;
        var budget = ComputeWaveBudget(wave);

        enemiesToSpawn = budget.totalEnemies;
        enemiesRemaining = budget.totalEnemies;
        maxConcurrent = budget.maxConcurrent;
        spawnInterval = budget.spawnInterval;
        spawnTimer = 0f;
        waveActive = true;

        if (wave % 5 == 0)
        {
            eliteChance = 0.25f;
        }
        else
        {
            eliteChance = 0.08f + wave * 0.01f;
        }

        OnWaveStarted?.Invoke(currentWave);
    }

    WaveBudget ComputeWaveBudget(int wave)
    {
        return new WaveBudget
        {
            totalEnemies = Mathf.RoundToInt(5 + wave * 1.8f),
            maxConcurrent = Mathf.Min(12, 4 + Mathf.FloorToInt(wave / 1.5f)),
            spawnInterval = Mathf.Max(0.4f, 1.3f - wave * 0.05f)
        };
    }

    void SpawnEnemy()
    {
        if (enemyData == null) return;

        EnemyController enemy = GetPooledEnemy();
        if (enemy == null) return;

        bool isElite = UnityEngine.Random.value < eliteChance;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : new Vector3(8f, 0, 0);
        pos.x = UnityEngine.Random.Range(spawnXMin, spawnXMax);
        pos.y = UnityEngine.Random.Range(-spawnYVariance, spawnYVariance);
        enemy.transform.position = pos;

        enemy.Setup(enemyData, currentWave, isElite);
        enemiesToSpawn--;
    }

    EnemyController GetPooledEnemy()
    {
        foreach (var enemy in pooledEnemies)
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                return enemy;
            }
        }

        GameObject enemyObj = new GameObject("Enemy_" + pooledEnemies.Count);
        enemyObj.transform.SetParent(transform);
        enemyObj.transform.localScale = Vector3.one * 0.4f;
        enemyObj.SetActive(false);

        EnemyController controller = enemyObj.AddComponent<EnemyController>();
        enemyObj.AddComponent<AttackAnimator>();

        pooledEnemies.Add(controller);
        return controller;
    }

    void HandleEnemyDeath(EnemyController enemy)
    {
        enemiesRemaining--;

        enemy.Recycle();

        if (enemiesRemaining <= 0)
        {
            waveActive = false;
            OnWaveCompleted?.Invoke(currentWave);

            if (currentWave % 5 == 0)
            {
                CameraEffects.Instance?.TriggerBossKillEffect();
            }

            StartWave(currentWave + 1);
        }
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetEnemiesRemaining()
    {
        return enemiesRemaining;
    }

    public int GetTotalEnemiesInWave()
    {
        return enemiesToSpawn + enemiesRemaining;
    }

    void OnDestroy()
    {
        if (combatManager != null)
        {
            combatManager.OnEnemyDeath -= HandleEnemyDeath;
        }
    }

    struct WaveBudget
    {
        public int totalEnemies;
        public int maxConcurrent;
        public float spawnInterval;
    }
}
