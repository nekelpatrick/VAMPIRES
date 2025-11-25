using UnityEngine;

public class EnemyController : CombatEntity
{
    public bool IsElite { get; private set; }
    public int WaveNumber { get; private set; }

    private CombatManager combatManager;
    private Renderer meshRenderer;
    private EnemyData enemyData;
    private float baseScale = 0.4f;

    protected override void Awake()
    {
        base.Awake();
        meshRenderer = GetComponentInChildren<Renderer>();
        facingDirection = -1;
    }

    public void Setup(EnemyData data, int wave, bool elite)
    {
        enemyData = data;
        WaveNumber = wave;
        IsElite = elite;

        CombatStats scaledStats = CalculateWaveStats(data.baseStats, wave, elite);
        Initialize(scaledStats);

        if (meshRenderer != null)
        {
            meshRenderer.material.color = elite ? data.eliteColor : data.normalColor;
        }

        float scale = baseScale * (elite ? 1.4f : 1f);
        transform.localScale = new Vector3(-scale, scale, scale);

        combatManager = CombatManager.Instance;
        if (combatManager != null)
        {
            combatManager.RegisterEnemy(this);
            currentTarget = combatManager.GetThrall();
        }

        gameObject.SetActive(true);
    }

    CombatStats CalculateWaveStats(CombatStats baseStats, int wave, bool elite)
    {
        float healthBase = baseStats.maxHealth * Mathf.Pow(1.08f, wave - 1);
        float attackBase = baseStats.attack * Mathf.Pow(1.05f, wave - 1);
        float defenseBase = baseStats.defense + wave * 0.4f;
        float speedBase = baseStats.speed + wave * 0.04f;
        float eliteBoost = elite ? 1.5f : 1f;

        return new CombatStats
        {
            maxHealth = healthBase * eliteBoost,
            attack = attackBase * eliteBoost,
            defense = defenseBase * eliteBoost,
            speed = speedBase * (elite ? 1.25f : 1f)
        };
    }

    protected override void FindTarget()
    {
        if (combatManager == null) return;
        currentTarget = combatManager.GetThrall();
    }

    public override void PerformAction()
    {
        if (currentTarget != null && currentTarget.IsAlive)
        {
            Attack(currentTarget);
        }
        else
        {
            FindTarget();
            if (currentTarget != null && currentTarget.IsAlive)
            {
                Attack(currentTarget);
            }
        }
    }

    void Attack(CombatEntity target)
    {
        TriggerAttackEvent();
        int damage = CombatMath.ComputeDamage(Stats.attack, target.Stats.defense);
        damage = CombatMath.ApplyVariance(damage, 0.2f, Random.value);
        target.TakeDamage(damage);

        DamageNumberSpawner spawner = DamageNumberSpawner.Instance;
        if (spawner != null)
        {
            Vector3 pos = target.transform.position + Vector3.up * 0.8f;
            spawner.Spawn(damage, pos, damage > Stats.attack * 1.3f);
        }
    }

    protected override void Die()
    {
        base.Die();
        if (combatManager != null)
        {
            combatManager.UnregisterEnemy(this);
            combatManager.OnEnemyKilled(this);
        }

        CoinDropper.SpawnCoins(transform.position, WaveNumber, IsElite);
    }

    public void Recycle()
    {
        gameObject.SetActive(false);
        CurrentHealth = 0;
        ActionPoints = 0;
        IsElite = false;
        WaveNumber = 0;
        currentTarget = null;
        isInRange = false;
    }
}
