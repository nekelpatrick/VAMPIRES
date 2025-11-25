using System;
using System.Collections.Generic;
using UnityEngine;

public class HordeSpawner : MonoBehaviour
{
    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;

    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnXMin = 5f;
    [SerializeField] private float spawnXMax = 8f;
    [SerializeField] private float spawnYVariance = 2f;
    [SerializeField] private float eliteChance = 0.1f;

    private CombatManager combatManager;
    private List<EnemyController> pooledEnemies = new List<EnemyController>();
    private GameObject enemyTemplate;

    private int currentWave;
    private int enemiesRemaining;
    private int enemiesToSpawn;
    private int maxConcurrent;
    private float spawnInterval;
    private float spawnTimer;
    private bool waveActive;

    void Awake()
    {
        CreateEnemyTemplate();
        CreateDefaultEnemyData();
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

    void CreateEnemyTemplate()
    {
        enemyTemplate = new GameObject("EnemyTemplate");
        enemyTemplate.SetActive(false);
        enemyTemplate.transform.SetParent(transform);
        enemyTemplate.tag = "Enemy";

        enemyTemplate.AddComponent<EnemyController>();
        enemyTemplate.AddComponent<AttackAnimator>();

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(enemyTemplate.transform);
        body.transform.localPosition = new Vector3(0, 1.25f, 0);
        body.transform.localScale = Vector3.one;
        body.GetComponent<Renderer>().material.color = new Color(0.8f, 0.2f, 0.2f);
        Destroy(body.GetComponent<Collider>());

        CreateEnemyHealthBar(enemyTemplate.transform);

        for (int i = 0; i < 15; i++)
        {
            GameObject clone = Instantiate(enemyTemplate, transform);
            clone.SetActive(false);
            pooledEnemies.Add(clone.GetComponent<EnemyController>());
        }
    }

    void CreateEnemyHealthBar(Transform parent)
    {
        GameObject hpBarRoot = new GameObject("HPBar");
        hpBarRoot.transform.SetParent(parent);
        hpBarRoot.transform.localPosition = new Vector3(0, 3.5f, 0);
        hpBarRoot.transform.localScale = Vector3.one * 2.5f;

        GameObject bgQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgQuad.name = "Background";
        bgQuad.transform.SetParent(hpBarRoot.transform);
        bgQuad.transform.localPosition = Vector3.zero;
        bgQuad.transform.localScale = new Vector3(1.2f, 0.15f, 1);
        bgQuad.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        Destroy(bgQuad.GetComponent<Collider>());

        GameObject fillQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fillQuad.name = "Fill";
        fillQuad.transform.SetParent(hpBarRoot.transform);
        fillQuad.transform.localPosition = new Vector3(0, 0, -0.01f);
        fillQuad.transform.localScale = new Vector3(1.1f, 0.1f, 1);
        fillQuad.GetComponent<Renderer>().material.color = new Color(0.8f, 0.2f, 0.2f);
        Destroy(fillQuad.GetComponent<Collider>());

        hpBarRoot.AddComponent<WorldSpaceHealthBar>();
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
            enemyData.normalColor = new Color(0.8f, 0.2f, 0.2f);
            enemyData.eliteColor = new Color(0.9f, 0.1f, 0.4f);
        }
    }

    void Update()
    {
        if (!waveActive) return;
        if (enemiesToSpawn <= 0) return;
        if (combatManager.GetActiveEnemyCount() >= maxConcurrent) return;

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

        OnWaveStarted?.Invoke(currentWave);
    }

    WaveBudget ComputeWaveBudget(int wave)
    {
        return new WaveBudget
        {
            totalEnemies = Mathf.RoundToInt(6 + wave * 1.5f),
            maxConcurrent = Mathf.Min(10, 4 + Mathf.FloorToInt(wave / 1.5f)),
            spawnInterval = Mathf.Max(0.45f, 1.4f - wave * 0.06f)
        };
    }

    void SpawnEnemy()
    {
        if (enemyData == null) return;

        EnemyController enemy = GetPooledEnemy();
        if (enemy == null) return;

        bool isElite = UnityEngine.Random.value < eliteChance;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
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

        GameObject clone = Instantiate(enemyTemplate, transform);
        EnemyController newEnemy = clone.GetComponent<EnemyController>();
        pooledEnemies.Add(newEnemy);
        return newEnemy;
    }

    void HandleEnemyDeath(EnemyController enemy)
    {
        enemiesRemaining--;

        enemy.Recycle();

        if (enemiesRemaining <= 0)
        {
            waveActive = false;
            OnWaveCompleted?.Invoke(currentWave);
            StartWave(currentWave + 1);
        }
    }

    public int GetCurrentWave()
    {
        return currentWave;
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
