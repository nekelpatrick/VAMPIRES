using System;
using UnityEngine;

public class ThrallController : CombatEntity
{
    public static event Action<int, int> OnXPGained;
    public static event Action<int> OnLevelUp;
    public static event Action<int> OnPowerScoreChanged;

    [SerializeField] private ThrallData thrallData;

    private CombatManager combatManager;

    public int ThrallLevel { get; private set; } = 1;
    public int CurrentXP { get; private set; } = 0;
    public int XPToNextLevel => CalculateXPForLevel(ThrallLevel + 1);
    public int PowerScore => CalculatePowerScore();

    protected override void Awake()
    {
        base.Awake();
        CreateDefaultData();
        facingDirection = 1;
    }

    int CalculateXPForLevel(int level)
    {
        return Mathf.RoundToInt(100 * Mathf.Pow(level, 1.5f));
    }

    int CalculatePowerScore()
    {
        return Mathf.RoundToInt(
            (Stats.attack * 2f) + 
            Stats.defense + 
            (Stats.maxHealth / 10f) + 
            (Stats.speed * 100f)
        );
    }

    public void GainXP(int amount)
    {
        if (amount <= 0) return;

        int previousXP = CurrentXP;
        CurrentXP += amount;

        OnXPGained?.Invoke(amount, CurrentXP);

        while (CurrentXP >= XPToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        CurrentXP -= XPToNextLevel;
        ThrallLevel++;

        CombatStats currentStats = Stats;
        currentStats.attack += 2f;
        currentStats.defense += 1f;
        currentStats.maxHealth += 15f;

        float healthPercent = CurrentHealth / Stats.maxHealth;
        Initialize(currentStats);
        CurrentHealth = Stats.maxHealth * healthPercent;

        OnLevelUp?.Invoke(ThrallLevel);
        OnPowerScoreChanged?.Invoke(PowerScore);

        Debug.Log($"[Thrall] LEVEL UP! Now level {ThrallLevel}");
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
        int damage = CombatMath.ApplyVariance(baseDamage, 0.25f, UnityEngine.Random.value);

        bool isCritical = UnityEngine.Random.value < 0.15f;
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
