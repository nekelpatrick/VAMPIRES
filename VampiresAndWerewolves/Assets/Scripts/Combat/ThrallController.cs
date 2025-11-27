using System;
using UnityEngine;

public class ThrallController : CombatEntity
{
    public static event Action<int, int> OnXPGained;
    public static event Action<int> OnLevelUp;
    public static event Action<int> OnPowerScoreChanged;

    [SerializeField] private ThrallData thrallData;

    private GameObject visualInstance;
    private Animator animator;

    static readonly int AttackTrigger = Animator.StringToHash("Attack");
    static readonly int HitTrigger = Animator.StringToHash("Hit");
    static readonly int DieTrigger = Animator.StringToHash("Die");

    private CombatManager combatManager;

    public int ThrallLevel { get; private set; } = 1;
    public int CurrentXP { get; private set; } = 0;
    public int XPToNextLevel => CalculateXPForLevel(ThrallLevel + 1);
    public int PowerScore => CalculatePowerScore();

    protected override void Awake()
    {
        base.Awake();
        CreateDefaultData();
        BuildVisual();
        facingDirection = 1;
        transform.rotation = Quaternion.Euler(0, 90f, 0);
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
        if (thrallData != null) return;

        ThrallData resource = Resources.Load<ThrallData>("Thralls/WerewolfData");
        if (resource != null)
        {
            thrallData = Instantiate(resource);
            return;
        }

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
        GameObject prefab = Resources.Load<GameObject>("Characters/WerewolfModel");
        if (prefab != null)
        {
            thrallData.visualPrefab = prefab;
        }
    }

    void BuildVisual()
    {
        if (visualInstance != null)
        {
            Destroy(visualInstance);
            visualInstance = null;
        }
        else
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child == null) continue;
                if (child.name.StartsWith("HPBar", StringComparison.OrdinalIgnoreCase)) continue;
                Destroy(child.gameObject);
            }
        }

        if (thrallData != null && thrallData.visualPrefab != null)
        {
            visualInstance = Instantiate(thrallData.visualPrefab, transform);
        }
        else
        {
            visualInstance = new GameObject("WerewolfVisual");
            visualInstance.transform.SetParent(transform, false);
            UnitVisualFactory.CreateWerewolfVisual(visualInstance.transform);
        }

        visualInstance.transform.localPosition = thrallData != null ? thrallData.visualOffset : Vector3.zero;
        Vector3 scale = thrallData != null ? thrallData.visualScale : Vector3.one;
        visualInstance.transform.localScale = Vector3.Scale(visualInstance.transform.localScale, scale);
        visualInstance.name = "Visual";

        animator = visualInstance.GetComponentInChildren<Animator>();
        if (animator == null && thrallData != null && thrallData.animatorController != null)
        {
            animator = visualInstance.AddComponent<Animator>();
        }

        if (animator != null && thrallData != null && thrallData.animatorController != null)
        {
            animator.runtimeAnimatorController = thrallData.animatorController;
        }

        if (animator != null)
        {
            animator.Update(0f);
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
        if (animator != null)
        {
            animator.SetTrigger(AttackTrigger);
        }

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
        if (animator != null)
        {
            animator.SetTrigger(DieTrigger);
        }
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

        if (animator != null && IsAlive && amount > 0)
        {
            animator.SetTrigger(HitTrigger);
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

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
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
