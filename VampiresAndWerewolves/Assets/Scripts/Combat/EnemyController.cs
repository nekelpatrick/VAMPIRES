using UnityEngine;

public class EnemyController : CombatEntity
{
    public bool IsElite { get; private set; }
    public int WaveNumber { get; private set; }
    public EnemyType EnemyType { get; private set; }

    private CombatManager combatManager;
    private EnemyData enemyData;
    private float baseScale = 0.4f;

    protected override void Awake()
    {
        base.Awake();
        facingDirection = -1;
    }

    public void Setup(EnemyData data, int wave, bool elite)
    {
        enemyData = data;
        WaveNumber = wave;
        IsElite = elite;

        EnemyType = GetRandomEnemyType(wave);

        CombatStats scaledStats = CalculateWaveStats(data.baseStats, wave, elite);
        Initialize(scaledStats);

        CreateVisuals();

        float scale = baseScale * (elite ? 1.5f : 1f);
        transform.localScale = new Vector3(-scale, scale, scale);

        combatManager = CombatManager.Instance;
        if (combatManager != null)
        {
            combatManager.RegisterEnemy(this);
            currentTarget = combatManager.GetThrall();
        }

        gameObject.SetActive(true);
    }

    EnemyType GetRandomEnemyType(int wave)
    {
        float rand = Random.value;

        if (wave >= 10 && rand < 0.2f) return EnemyType.Demon;
        if (wave >= 5 && rand < 0.3f) return EnemyType.Wraith;
        if (rand < 0.4f) return EnemyType.Skeleton;
        return EnemyType.Ghoul;
    }

    void CreateVisuals()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name != "HPBar")
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        UnitVisualFactory.CreateEnemyVisual(transform, EnemyType);

        if (IsElite)
        {
            AddEliteEffects();
        }

        CreateHealthBar();
    }

    void AddEliteEffects()
    {
        GameObject aura = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        aura.name = "EliteAura";
        aura.transform.SetParent(transform);
        aura.transform.localPosition = new Vector3(0, 1f, 0);
        aura.transform.localScale = Vector3.one * 3f;

        Renderer r = aura.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        Color auraColor;
        switch (EnemyType)
        {
            case EnemyType.Demon:
                auraColor = new Color(1f, 0.3f, 0.1f, 0.15f);
                break;
            case EnemyType.Wraith:
                auraColor = new Color(0.3f, 0.5f, 0.8f, 0.15f);
                break;
            case EnemyType.Skeleton:
                auraColor = new Color(0.8f, 0.8f, 0.6f, 0.15f);
                break;
            default:
                auraColor = new Color(0.5f, 0.8f, 0.3f, 0.15f);
                break;
        }

        mat.color = auraColor;
        r.material = mat;
        Object.Destroy(aura.GetComponent<Collider>());
    }

    void CreateHealthBar()
    {
        GameObject existingBar = transform.Find("HPBar")?.gameObject;
        if (existingBar != null) return;

        GameObject hpBarRoot = new GameObject("HPBar");
        hpBarRoot.transform.SetParent(transform);
        hpBarRoot.transform.localPosition = new Vector3(0, 4f, 0);
        hpBarRoot.transform.localScale = Vector3.one * 2f;

        Color barColor;
        switch (EnemyType)
        {
            case EnemyType.Demon:
                barColor = new Color(0.9f, 0.3f, 0.2f);
                break;
            case EnemyType.Wraith:
                barColor = new Color(0.4f, 0.6f, 0.9f);
                break;
            case EnemyType.Skeleton:
                barColor = new Color(0.8f, 0.75f, 0.6f);
                break;
            default:
                barColor = new Color(0.5f, 0.7f, 0.4f);
                break;
        }

        if (IsElite)
        {
            barColor = Color.Lerp(barColor, new Color(1f, 0.8f, 0.2f), 0.3f);
        }

        GameObject bgQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgQuad.name = "Background";
        bgQuad.transform.SetParent(hpBarRoot.transform);
        bgQuad.transform.localPosition = Vector3.zero;
        bgQuad.transform.localScale = new Vector3(1.2f, 0.15f, 1);

        Renderer bgRenderer = bgQuad.GetComponent<Renderer>();
        Material bgMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        bgMat.color = new Color(0.05f, 0.03f, 0.03f, 0.85f);
        bgRenderer.material = bgMat;
        Object.Destroy(bgQuad.GetComponent<Collider>());

        GameObject fillQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fillQuad.name = "Fill";
        fillQuad.transform.SetParent(hpBarRoot.transform);
        fillQuad.transform.localPosition = new Vector3(0, 0, -0.01f);
        fillQuad.transform.localScale = new Vector3(1.1f, 0.1f, 1);

        Renderer fillRenderer = fillQuad.GetComponent<Renderer>();
        Material fillMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        fillMat.color = barColor;
        fillRenderer.material = fillMat;
        Object.Destroy(fillQuad.GetComponent<Collider>());

        hpBarRoot.AddComponent<WorldSpaceHealthBar>();
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

        BloodParticleSystem.Instance?.SpawnBloodSplash(
            target.transform.position + Vector3.up * 0.8f,
            (target.transform.position - transform.position).normalized,
            damage
        );

        ScreenEffects.Instance?.FlashOnThrallHit();
        CameraEffects.Instance?.ShakeOnDamage(damage);

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

        SpawnDeathEffects();

        if (combatManager != null)
        {
            combatManager.UnregisterEnemy(this);
            combatManager.OnEnemyKilled(this);
        }

        GothicHUD.Instance?.OnEnemyKilled();

        CoinDropper.SpawnCoins(transform.position, WaveNumber, IsElite);

        if (IsElite)
        {
            CameraEffects.Instance?.TriggerSlowMo(0.3f);
        }
    }

    void SpawnDeathEffects()
    {
        BloodParticleSystem blood = BloodParticleSystem.Instance;
        if (blood != null)
        {
            int intensity = IsElite ? 30 : 15;
            blood.SpawnBloodSplash(
                transform.position + Vector3.up * 0.6f,
                Vector3.up + Random.insideUnitSphere * 0.5f,
                intensity
            );

            blood.SpawnSlashEffect(
                transform.position + Vector3.up * 0.8f,
                Vector3.right
            );
        }

        CameraEffects.Instance?.ShakeOnKill();
        ScreenEffects.Instance?.FlashOnKill();
    }

    public void Recycle()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }

        gameObject.SetActive(false);
        CurrentHealth = 0;
        ActionPoints = 0;
        IsElite = false;
        WaveNumber = 0;
        currentTarget = null;
        isInRange = false;
    }
}
