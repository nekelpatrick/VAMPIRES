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
}

