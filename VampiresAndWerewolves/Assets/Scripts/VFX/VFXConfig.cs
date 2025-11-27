using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "VFXConfig", menuName = "Vampires/VFXConfig")]
public class VFXConfig : ScriptableObject
{
    [Header("Blood Effects")]
    public VisualEffectAsset bloodSplash;
    public VisualEffectAsset bloodDrip;
    public VisualEffectAsset deathExplosion;
    public VisualEffectAsset groundDecal;

    [Header("Ability Effects")]
    public VisualEffectAsset lifestealEffect;
    public VisualEffectAsset bleedEffect;
    public VisualEffectAsset stunEffect;
    public VisualEffectAsset rageAura;
    public VisualEffectAsset howlWave;

    [Header("Fallback Settings")]
    public bool useFallbackIfMissing = true;
}

