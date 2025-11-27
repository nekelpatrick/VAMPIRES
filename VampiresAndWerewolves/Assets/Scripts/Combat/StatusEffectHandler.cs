using UnityEngine;
using System.Collections.Generic;

public class StatusEffectHandler : MonoBehaviour
{
    private CombatEntity entity;
    private List<ActiveStatusEffect> activeEffects = new();

    private struct ActiveStatusEffect
    {
        public StatusEffectData data;
        public float remainingDuration;
        public float nextTickTime;
    }

    void Awake()
    {
        entity = GetComponent<CombatEntity>();
    }

    void Update()
    {
        if (entity == null || !entity.IsAlive) return;

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            effect.remainingDuration -= Time.deltaTime;

            if (effect.data.tickInterval > 0 && Time.time >= effect.nextTickTime)
            {
                ApplyTick(effect.data);
                effect.nextTickTime = Time.time + effect.data.tickInterval;
            }

            if (effect.remainingDuration <= 0)
            {
                RemoveEffect(effect.data.type);
                activeEffects.RemoveAt(i);
            }
            else
            {
                activeEffects[i] = effect;
            }
        }
    }

    public void ApplyEffect(StatusEffectData data)
    {
        var existing = activeEffects.FindIndex(e => e.type == data.type);
        if (existing >= 0)
        {
            var effect = activeEffects[existing];
            effect.remainingDuration = data.duration;
            activeEffects[existing] = effect;
            return;
        }

        activeEffects.Add(new ActiveStatusEffect
        {
            data = data,
            remainingDuration = data.duration,
            nextTickTime = Time.time + data.tickInterval
        });

        OnEffectApplied(data);
    }

    private void OnEffectApplied(StatusEffectData data)
    {
        switch (data.type)
        {
            case AbilityType.Bleed:
                VFXManager.Instance?.SpawnBleedEffect(transform);
                break;

            case AbilityType.Stun:
                VFXManager.Instance?.SpawnStunEffect(transform.position);
                break;

            case AbilityType.Rage:
                VFXManager.Instance?.SpawnRageAura(transform, data.duration);
                break;
        }
    }

    private void ApplyTick(StatusEffectData data)
    {
        if (entity == null || !entity.IsAlive) return;

        switch (data.type)
        {
            case AbilityType.Bleed:
                int bleedDamage = Mathf.RoundToInt(data.magnitude);
                entity.TakeDamage(bleedDamage);
                break;
        }
    }

    private void RemoveEffect(AbilityType type)
    {
    }

    public bool HasEffect(AbilityType type)
    {
        return activeEffects.Exists(e => e.data.type == type && e.remainingDuration > 0);
    }

    public float GetEffectMagnitude(AbilityType type)
    {
        var effect = activeEffects.Find(e => e.data.type == type);
        return effect.data.magnitude;
    }

    public bool IsStunned()
    {
        return HasEffect(AbilityType.Stun);
    }

    public float GetRageBonus()
    {
        if (!HasEffect(AbilityType.Rage)) return 0f;
        return GetEffectMagnitude(AbilityType.Rage);
    }
}

