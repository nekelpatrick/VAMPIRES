using System;
using UnityEngine;

public abstract class CombatEntity : MonoBehaviour
{
    public event Action<CombatEntity> OnDeath;
    public event Action<int> OnDamageTaken;
    public event Action OnAttack;
    public event Action<AbilityType, CombatEntity> OnAbilityTriggered;

    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float attackRange = 1.5f;

    public CombatStats Stats { get; protected set; }
    public float CurrentHealth { get; protected set; }
    public float ActionPoints { get; protected set; }
    public bool IsAlive => CurrentHealth > 0;

    protected CombatEntity currentTarget;
    protected bool isInRange;
    protected int facingDirection = 1;

    protected virtual void Awake()
    {
        ActionPoints = 0f;
    }

    public virtual void Initialize(CombatStats stats)
    {
        Stats = stats;
        CurrentHealth = stats.maxHealth;
        ActionPoints = 0f;
        isInRange = false;
    }

    protected virtual void Update()
    {
        if (!IsAlive) return;
        UpdateMovement();
    }

    protected virtual void UpdateMovement()
    {
        if (currentTarget == null || !currentTarget.IsAlive)
        {
            FindTarget();
            isInRange = false;
        }

        if (currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        isInRange = distance <= attackRange;

        if (!isInRange)
        {
            Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            facingDirection = direction.x > 0 ? 1 : -1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDirection, transform.localScale.y, transform.localScale.z);
        }
    }

    protected abstract void FindTarget();

    public void AccumulateAP(float deltaTime)
    {
        if (!IsAlive) return;
        ActionPoints += Stats.speed * deltaTime * 100f;
    }

    public bool CanAct()
    {
        return IsAlive && ActionPoints >= 100f && isInRange;
    }

    public void ConsumeAP()
    {
        ActionPoints -= 100f;
    }

    public abstract void PerformAction();

    public virtual void TakeDamage(int damage)
    {
        if (!IsAlive) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        OnDamageTaken?.Invoke(damage);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(int amount)
    {
        if (!IsAlive) return;
        CurrentHealth = Mathf.Min(Stats.maxHealth, CurrentHealth + amount);
    }

    protected void TriggerAttackEvent()
    {
        OnAttack?.Invoke();
    }

    public void TriggerAbility(AbilityType type, CombatEntity target = null)
    {
        OnAbilityTriggered?.Invoke(type, target ?? currentTarget);
        SpawnAbilityVFX(type, target);
    }

    private void SpawnAbilityVFX(AbilityType type, CombatEntity target)
    {
        if (VFXManager.Instance == null) return;

        switch (type)
        {
            case AbilityType.Lifesteal:
                if (target != null)
                    VFXManager.Instance.SpawnLifestealEffect(target.transform.position, transform.position);
                break;

            case AbilityType.Bleed:
                if (target != null)
                    VFXManager.Instance.SpawnBleedEffect(target.transform);
                break;

            case AbilityType.Stun:
                if (target != null)
                    VFXManager.Instance.SpawnStunEffect(target.transform.position);
                break;

            case AbilityType.Rage:
                VFXManager.Instance.SpawnRageAura(transform, 5f);
                break;

            case AbilityType.Howl:
                VFXManager.Instance.SpawnHowlWave(transform.position, 5f);
                break;
        }
    }

    protected virtual void Die()
    {
        BloodParticleSystem.Instance?.SpawnDeathExplosion(transform.position);
        OnDeath?.Invoke(this);
    }

    public void SetTarget(CombatEntity target)
    {
        currentTarget = target;
    }

    public CombatEntity GetTarget()
    {
        return currentTarget;
    }
}
