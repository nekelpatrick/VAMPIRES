using UnityEngine;

public enum QuestType
{
    KillEnemies,
    ReachWave,
    DealDamage,
    WinPvP,
    GetCombo,
    EarnCurrency,
    SurviveWaves
}

[CreateAssetMenu(fileName = "QuestData", menuName = "Vampires/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questId;
    public string questName;
    public string description;
    public QuestType questType;
    public int targetValue;
    public int duskenReward;
    public int bloodShardsReward;
    public float adBonusMultiplier = 2f;
    public Sprite icon;

    public string GetProgressText(int current)
    {
        return $"{current}/{targetValue}";
    }

    public string GetRewardText()
    {
        if (bloodShardsReward > 0)
        {
            return $"+{bloodShardsReward} Blood Shards";
        }
        return $"+{duskenReward} Dusken";
    }
}

