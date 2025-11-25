using System;
using UnityEngine;

public abstract class CombatEntity : MonoBehaviour
{
    public event Action<CombatEntity> OnDeath;
    public event Action<int> OnDamageTaken;
    public event Action OnAttack;

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

    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        OnDamageTaken?.Invoke(damage);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    protected void TriggerAttackEvent()
    {
        OnAttack?.Invoke();
    }

    protected virtual void Die()
    {
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
