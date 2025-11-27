using UnityEngine;

[CreateAssetMenu(fileName = "ThrallData", menuName = "Vampires/ThrallData")]
public class ThrallData : ScriptableObject
{
    public string thrallName = "Werewolf";
    public CombatStats baseStats = new CombatStats
    {
        maxHealth = 500f,
        attack = 45f,
        defense = 12f,
        speed = 1.2f
    };
    public Color color = Color.blue;
    public GameObject visualPrefab;
    public RuntimeAnimatorController animatorController;
    public Vector3 visualOffset = new Vector3(0f, 0f, 0f);
    public Vector3 visualScale = Vector3.one;
}

