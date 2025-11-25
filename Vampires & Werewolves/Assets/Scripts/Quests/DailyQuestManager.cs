using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestProgress
{
    public string questId;
    public QuestType questType;
    public int targetValue;
    public int currentValue;
    public int duskenReward;
    public int bloodShardsReward;
    public bool isCompleted;
    public bool isClaimed;
    public bool adBonusClaimed;
    public string questName;
    public string description;
}

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance { get; private set; }

    public event Action<QuestProgress> OnQuestProgress;
    public event Action<QuestProgress> OnQuestCompleted;
    public event Action<QuestProgress> OnQuestClaimed;
    public event Action OnQuestsReset;

    [Header("Quest Definitions")]
    private List<QuestProgress> dailyQuests = new List<QuestProgress>();

    private string lastResetDate;
    private int totalKills;
    private int highestWave;
    private long totalDamage;
    private int pvpWins;
    private int highestCombo;

    private CombatManager combatManager;
    private HordeSpawner hordeSpawner;
    private CurrencyManager currencyManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        combatManager = CombatManager.Instance;
        hordeSpawner = UnityEngine.Object.FindFirstObjectByType<HordeSpawner>();
        currencyManager = CurrencyManager.Instance;

        SubscribeToEvents();
        CheckDailyReset();
    }

    void SubscribeToEvents()
    {
        if (combatManager != null)
        {
            combatManager.OnEnemyDeath += OnEnemyKilled;
        }

        if (hordeSpawner != null)
        {
            hordeSpawner.OnWaveStarted += OnWaveReached;
        }

        GothicHUD hud = GothicHUD.Instance;
        if (hud != null)
        {
            hud.OnComboChanged += OnComboUpdated;
        }
    }

    void CheckDailyReset()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        string savedDate = PlayerPrefs.GetString("DailyQuestDate", "");

        if (savedDate != today)
        {
            ResetDailyQuests();
            lastResetDate = today;
            PlayerPrefs.SetString("DailyQuestDate", today);
            PlayerPrefs.Save();
        }
        else
        {
            LoadQuestProgress();
        }
    }

    void ResetDailyQuests()
    {
        dailyQuests.Clear();
        totalKills = 0;
        highestWave = 0;
        totalDamage = 0;
        pvpWins = 0;
        highestCombo = 0;

        dailyQuests.Add(new QuestProgress
        {
            questId = "kill_100",
            questName = "Slay the Horde",
            description = "Kill 100 enemies",
            questType = QuestType.KillEnemies,
            targetValue = 100,
            duskenReward = 500,
            bloodShardsReward = 0
        });

        dailyQuests.Add(new QuestProgress
        {
            questId = "wave_20",
            questName = "Wave Crusher",
            description = "Reach wave 20",
            questType = QuestType.ReachWave,
            targetValue = 20,
            duskenReward = 1000,
            bloodShardsReward = 0
        });

        dailyQuests.Add(new QuestProgress
        {
            questId = "damage_50k",
            questName = "Blood Harvest",
            description = "Deal 50,000 damage",
            questType = QuestType.DealDamage,
            targetValue = 50000,
            duskenReward = 750,
            bloodShardsReward = 0
        });

        dailyQuests.Add(new QuestProgress
        {
            questId = "pvp_3",
            questName = "PvP Victor",
            description = "Win 3 PvP battles",
            questType = QuestType.WinPvP,
            targetValue = 3,
            duskenReward = 0,
            bloodShardsReward = 2
        });

        dailyQuests.Add(new QuestProgress
        {
            questId = "combo_50",
            questName = "Combo Master",
            description = "Get 50x combo",
            questType = QuestType.GetCombo,
            targetValue = 50,
            duskenReward = 300,
            bloodShardsReward = 0
        });

        SaveQuestProgress();
        OnQuestsReset?.Invoke();

        Debug.Log("[DailyQuests] Daily quests reset!");
    }

    void OnEnemyKilled(EnemyController enemy)
    {
        totalKills++;
        UpdateQuestProgress(QuestType.KillEnemies, totalKills);

        int damage = Mathf.RoundToInt(enemy.Stats.maxHealth);
        totalDamage += damage;
        UpdateQuestProgress(QuestType.DealDamage, (int)totalDamage);
    }

    void OnWaveReached(int wave)
    {
        if (wave > highestWave)
        {
            highestWave = wave;
            UpdateQuestProgress(QuestType.ReachWave, highestWave);
        }
    }

    void OnComboUpdated(int combo)
    {
        if (combo > highestCombo)
        {
            highestCombo = combo;
            UpdateQuestProgress(QuestType.GetCombo, highestCombo);
        }
    }

    public void OnPvPWin()
    {
        pvpWins++;
        UpdateQuestProgress(QuestType.WinPvP, pvpWins);
    }

    void UpdateQuestProgress(QuestType type, int value)
    {
        foreach (var quest in dailyQuests)
        {
            if (quest.questType == type && !quest.isCompleted)
            {
                quest.currentValue = Mathf.Min(value, quest.targetValue);
                OnQuestProgress?.Invoke(quest);

                if (quest.currentValue >= quest.targetValue)
                {
                    quest.isCompleted = true;
                    OnQuestCompleted?.Invoke(quest);
                    Debug.Log($"[DailyQuests] Quest completed: {quest.questName}");
                }
            }
        }

        SaveQuestProgress();
    }

    public void ClaimQuest(string questId, bool watchedAd)
    {
        QuestProgress quest = dailyQuests.Find(q => q.questId == questId);
        if (quest == null || !quest.isCompleted || quest.isClaimed)
        {
            return;
        }

        int duskenAmount = quest.duskenReward;
        int shardsAmount = quest.bloodShardsReward;

        if (watchedAd && !quest.adBonusClaimed)
        {
            duskenAmount *= 2;
            shardsAmount *= 2;
            quest.adBonusClaimed = true;
        }

        if (currencyManager != null)
        {
            if (duskenAmount > 0)
            {
                currencyManager.AddDuskenCoin(duskenAmount);
            }
            if (shardsAmount > 0)
            {
                currencyManager.AddBloodShards(shardsAmount);
            }
        }

        quest.isClaimed = true;
        SaveQuestProgress();

        OnQuestClaimed?.Invoke(quest);
        Debug.Log($"[DailyQuests] Claimed quest: {quest.questName} (+{duskenAmount} Dusken, +{shardsAmount} Shards)");
    }

    void SaveQuestProgress()
    {
        string json = JsonUtility.ToJson(new QuestSaveData { quests = dailyQuests });
        PlayerPrefs.SetString("DailyQuestProgress", json);
        PlayerPrefs.SetInt("TotalKills", totalKills);
        PlayerPrefs.SetInt("HighestWave", highestWave);
        PlayerPrefs.SetString("TotalDamage", totalDamage.ToString());
        PlayerPrefs.SetInt("PvPWins", pvpWins);
        PlayerPrefs.SetInt("HighestCombo", highestCombo);
        PlayerPrefs.Save();
    }

    void LoadQuestProgress()
    {
        string json = PlayerPrefs.GetString("DailyQuestProgress", "");
        if (!string.IsNullOrEmpty(json))
        {
            QuestSaveData data = JsonUtility.FromJson<QuestSaveData>(json);
            if (data != null && data.quests != null)
            {
                dailyQuests = data.quests;
            }
        }

        totalKills = PlayerPrefs.GetInt("TotalKills", 0);
        highestWave = PlayerPrefs.GetInt("HighestWave", 0);
        long.TryParse(PlayerPrefs.GetString("TotalDamage", "0"), out totalDamage);
        pvpWins = PlayerPrefs.GetInt("PvPWins", 0);
        highestCombo = PlayerPrefs.GetInt("HighestCombo", 0);
    }

    public List<QuestProgress> GetDailyQuests()
    {
        return dailyQuests;
    }

    public int GetUnclaimedCount()
    {
        int count = 0;
        foreach (var quest in dailyQuests)
        {
            if (quest.isCompleted && !quest.isClaimed)
            {
                count++;
            }
        }
        return count;
    }

    public float GetOverallProgress()
    {
        if (dailyQuests.Count == 0) return 0;

        float totalProgress = 0;
        foreach (var quest in dailyQuests)
        {
            totalProgress += (float)quest.currentValue / quest.targetValue;
        }
        return totalProgress / dailyQuests.Count;
    }

    void OnDestroy()
    {
        if (combatManager != null)
        {
            combatManager.OnEnemyDeath -= OnEnemyKilled;
        }

        if (hordeSpawner != null)
        {
            hordeSpawner.OnWaveStarted -= OnWaveReached;
        }
    }

    [Serializable]
    private class QuestSaveData
    {
        public List<QuestProgress> quests;
    }
}

