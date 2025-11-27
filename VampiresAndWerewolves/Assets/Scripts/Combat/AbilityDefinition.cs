using UnityEngine;

public enum AbilityTrigger
{
    OnAttack,
    OnHit,
    OnKill,
    OnLowHealth,
    Active
}

[CreateAssetMenu(fileName = "AbilityDefinition", menuName = "Vampires/AbilityDefinition")]
public class AbilityDefinition : ScriptableObject
{
    public string abilityId;
    public AbilityType type;
    public AbilityTrigger trigger;

    [Range(0f, 1f)]
    public float chance = 1f;

    public float magnitude;
    public float duration;
    public float cooldown;

    [Header("Visual")]
    public Color effectColor = Color.red;
    public string description;
}

