using UnityEngine;
using System.Collections.Generic;

public class AbilitySystem : MonoBehaviour
{
    public static AbilitySystem Instance { get; private set; }

    [SerializeField] private List<AbilityDefinition> abilityDefinitions = new();

    private Dictionary<AbilityType, AbilityDefinition> definitionLookup = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeDefinitions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDefinitions()
    {
        foreach (var def in abilityDefinitions)
        {
            if (def != null && !definitionLookup.ContainsKey(def.type))
            {
                definitionLookup[def.type] = def;
            }
        }
    }

    public AbilityDefinition GetDefinition(AbilityType type)
    {
        return definitionLookup.TryGetValue(type, out var def) ? def : null;
    }

    public bool TryTriggerAbility(
        CombatEntity source,
        CombatEntity target,
        AbilityType type,
        AbilityTrigger trigger)
    {
        var def = GetDefinition(type);
        if (def == null) return false;
        if (def.trigger != trigger) return false;

        if (Random.value > def.chance) return false;

        ExecuteAbility(source, target, def);
        return true;
    }

    private void ExecuteAbility(CombatEntity source, CombatEntity target, AbilityDefinition def)
    {
        source.TriggerAbility(def.type, target);

        switch (def.type)
        {
            case AbilityType.Lifesteal:
                ApplyLifesteal(source, target, def.magnitude);
                break;

            case AbilityType.Bleed:
                ApplyBleed(target, def);
                break;

            case AbilityType.Stun:
                ApplyStun(target, def);
                break;

            case AbilityType.Rage:
                ApplyRage(source, def);
                break;

            case AbilityType.Howl:
                ApplyHowl(source, def);
                break;
        }
    }

    private void ApplyLifesteal(CombatEntity source, CombatEntity target, float percent)
    {
        int healAmount = Mathf.RoundToInt(source.Stats.attack * percent);
        source.Heal(healAmount);
    }

    private void ApplyBleed(CombatEntity target, AbilityDefinition def)
    {
        var executor = target.GetComponent<StatusEffectHandler>();
        if (executor == null)
        {
            executor = target.gameObject.AddComponent<StatusEffectHandler>();
        }

        executor.ApplyEffect(new StatusEffectData
        {
            type = AbilityType.Bleed,
            magnitude = def.magnitude,
            duration = def.duration,
            tickInterval = 1f
        });
    }

    private void ApplyStun(CombatEntity target, AbilityDefinition def)
    {
        var executor = target.GetComponent<StatusEffectHandler>();
        if (executor == null)
        {
            executor = target.gameObject.AddComponent<StatusEffectHandler>();
        }

        executor.ApplyEffect(new StatusEffectData
        {
            type = AbilityType.Stun,
            magnitude = 0,
            duration = def.duration,
            tickInterval = 0
        });
    }

    private void ApplyRage(CombatEntity source, AbilityDefinition def)
    {
        var executor = source.GetComponent<StatusEffectHandler>();
        if (executor == null)
        {
            executor = source.gameObject.AddComponent<StatusEffectHandler>();
        }

        executor.ApplyEffect(new StatusEffectData
        {
            type = AbilityType.Rage,
            magnitude = def.magnitude,
            duration = def.duration,
            tickInterval = 0
        });
    }

    private void ApplyHowl(CombatEntity source, AbilityDefinition def)
    {
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(source.transform.position, enemy.transform.position);
            if (distance <= 5f)
            {
                var executor = enemy.GetComponent<StatusEffectHandler>();
                if (executor == null)
                {
                    executor = enemy.gameObject.AddComponent<StatusEffectHandler>();
                }

                executor.ApplyEffect(new StatusEffectData
                {
                    type = AbilityType.Howl,
                    magnitude = def.magnitude,
                    duration = def.duration,
                    tickInterval = 0
                });
            }
        }
    }
}

public struct StatusEffectData
{
    public AbilityType type;
    public float magnitude;
    public float duration;
    public float tickInterval;
}

