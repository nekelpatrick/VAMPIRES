using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Vampires/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Horde Minion";
    public CombatStats baseStats = new CombatStats
    {
        maxHealth = 90f,
        attack = 14f,
        defense = 4f,
        speed = 1f
    };
    public Color normalColor = Color.red;
    public Color eliteColor = new Color(0.8f, 0.2f, 0.4f);
}

