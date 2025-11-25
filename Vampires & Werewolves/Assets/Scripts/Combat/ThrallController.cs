using UnityEngine;

public class ThrallController : CombatEntity
{
    [SerializeField] private ThrallData thrallData;

    private CombatManager combatManager;
    private Renderer meshRenderer;

    protected override void Awake()
    {
        base.Awake();
        meshRenderer = GetComponentInChildren<Renderer>();
        CreateDefaultData();
        facingDirection = 1;
    }

    void CreateDefaultData()
    {
        if (thrallData == null)
        {
            thrallData = ScriptableObject.CreateInstance<ThrallData>();
            thrallData.thrallName = "Werewolf";
            thrallData.baseStats = new CombatStats
            {
                maxHealth = 500f,
                attack = 45f,
                defense = 12f,
                speed = 1.2f
            };
            thrallData.color = new Color(0.2f, 0.4f, 0.9f);
        }
    }

    void Start()
    {
        Initialize(thrallData.baseStats);
        if (meshRenderer != null)
        {
            meshRenderer.material.color = thrallData.color;
        }

        combatManager = CombatManager.Instance;
        if (combatManager != null)
        {
            combatManager.RegisterThrall(this);
        }
    }

    protected override void FindTarget()
    {
        if (combatManager == null) return;
        currentTarget = combatManager.GetNearestEnemy(transform.position);
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
            Vector3 pos = target.transform.position + Vector3.up * 1.2f;
            spawner.Spawn(damage, pos, damage > Stats.attack * 1.3f);
        }
    }

    protected override void Die()
    {
        base.Die();
        if (combatManager != null)
        {
            combatManager.OnThrallDeath();
        }
    }

    void OnDestroy()
    {
        if (combatManager != null)
        {
            combatManager.UnregisterThrall(this);
        }
    }
}
