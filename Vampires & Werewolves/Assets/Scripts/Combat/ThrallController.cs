using UnityEngine;

public class ThrallController : CombatEntity
{
    [SerializeField] private ThrallData thrallData;

    private CombatManager combatManager;

    protected override void Awake()
    {
        base.Awake();
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
                attack = 55f,
                defense = 15f,
                speed = 1.3f
            };
            thrallData.color = new Color(0.3f, 0.25f, 0.35f);
        }
    }

    void Start()
    {
        Initialize(thrallData.baseStats);

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

        int baseDamage = CombatMath.ComputeDamage(Stats.attack, target.Stats.defense);
        int damage = CombatMath.ApplyVariance(baseDamage, 0.25f, Random.value);

        bool isCritical = Random.value < 0.15f;
        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * 1.75f);
        }

        target.TakeDamage(damage);

        Vector3 hitPos = target.transform.position + Vector3.up * 0.8f;
        Vector3 attackDir = (target.transform.position - transform.position).normalized;

        BloodParticleSystem blood = BloodParticleSystem.Instance;
        if (blood != null)
        {
            blood.SpawnBloodSplash(hitPos, attackDir, damage);
            blood.SpawnSlashEffect(hitPos, attackDir);
        }

        if (isCritical)
        {
            CameraEffects.Instance?.ShakeOnCritical();
            ScreenEffects.Instance?.FlashOnCritical();
            CameraEffects.Instance?.TriggerHitPause();
        }
        else
        {
            CameraEffects.Instance?.ShakeOnDamage(damage / 2);
        }

        DamageNumberSpawner spawner = DamageNumberSpawner.Instance;
        if (spawner != null)
        {
            spawner.Spawn(damage, hitPos, isCritical);
        }
    }

    protected override void Die()
    {
        base.Die();
        if (combatManager != null)
        {
            combatManager.OnThrallDeath();
        }

        ScreenEffects.Instance?.FlashRed(0.5f);
        CameraEffects.Instance?.Shake(0.3f);
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

        float healthRatio = CurrentHealth / Stats.maxHealth;
        if (healthRatio < 0.3f)
        {
            ScreenEffects.Instance?.FlashBloodBorder(0.1f);
        }
    }

    public void Revive()
    {
        CurrentHealth = Stats.maxHealth;
        ActionPoints = 0;
        gameObject.SetActive(true);

        if (combatManager != null)
        {
            combatManager.ResumeCombat();
        }

        ScreenEffects.Instance?.FlashGreen(0.3f);
        Debug.Log("[Thrall] Revived!");
    }

    void OnDestroy()
    {
        if (combatManager != null)
        {
            combatManager.UnregisterThrall(this);
        }
    }
}
