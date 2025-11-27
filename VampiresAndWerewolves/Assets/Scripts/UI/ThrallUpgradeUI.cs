using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UpgradeStat
{
    Attack,
    Defense,
    MaxHp,
    Speed
}

[Serializable]
public struct UpgradeCost
{
    public UpgradeStat stat;
    public int currentLevel;
    public int duskenCost;
    public int bloodShardCost;
    public float statIncrease;
}

public class ThrallUpgradeUI : MonoBehaviour
{
    public static ThrallUpgradeUI Instance { get; private set; }

    public event Action<UpgradeStat> OnUpgradeRequested;
    public event Action<UpgradeStat, bool> OnUpgradeCompleted;

    [Header("Stats Display")]
    private TextMeshProUGUI attackValueText;
    private TextMeshProUGUI defenseValueText;
    private TextMeshProUGUI maxHpValueText;
    private TextMeshProUGUI speedValueText;

    [Header("Level Display")]
    private TextMeshProUGUI attackLevelText;
    private TextMeshProUGUI defenseLevelText;
    private TextMeshProUGUI maxHpLevelText;
    private TextMeshProUGUI speedLevelText;

    [Header("Cost Display")]
    private TextMeshProUGUI attackCostText;
    private TextMeshProUGUI defenseCostText;
    private TextMeshProUGUI maxHpCostText;
    private TextMeshProUGUI speedCostText;

    [Header("Buttons")]
    private Button attackUpgradeBtn;
    private Button defenseUpgradeBtn;
    private Button maxHpUpgradeBtn;
    private Button speedUpgradeBtn;

    private CurrencyManager currencyManager;
    private ThrallController thrall;

    private int[] statLevels = new int[4];

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currencyManager = CurrencyManager.Instance;
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged += OnCurrencyChanged;
        }

        StartCoroutine(BindThrall());
    }

    System.Collections.IEnumerator BindThrall()
    {
        int attempts = 0;
        while (thrall == null && attempts < 20)
        {
            CombatManager combat = CombatManager.Instance;
            if (combat != null)
            {
                thrall = combat.GetThrall();
            }

            if (thrall == null)
            {
                yield return new WaitForSeconds(0.2f);
                attempts++;
            }
        }

        if (thrall != null)
        {
            UpdateStatsDisplay();
        }
    }

    void OnCurrencyChanged(int dusken, int shards)
    {
        UpdateButtonStates();
    }

    public void CreateUpgradePanel(Transform parent)
    {
        GameObject panel = new GameObject("ThrallUpgradePanel");
        panel.transform.SetParent(parent);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateStatRow(panel.transform, "ATTACK", UpgradeStat.Attack, 0, out attackValueText, out attackLevelText, out attackCostText, out attackUpgradeBtn);
        CreateStatRow(panel.transform, "DEFENSE", UpgradeStat.Defense, 1, out defenseValueText, out defenseLevelText, out defenseCostText, out defenseUpgradeBtn);
        CreateStatRow(panel.transform, "MAX HP", UpgradeStat.MaxHp, 2, out maxHpValueText, out maxHpLevelText, out maxHpCostText, out maxHpUpgradeBtn);
        CreateStatRow(panel.transform, "SPEED", UpgradeStat.Speed, 3, out speedValueText, out speedLevelText, out speedCostText, out speedUpgradeBtn);

        UpdateStatsDisplay();
        UpdateButtonStates();
    }

    void CreateStatRow(Transform parent, string statName, UpgradeStat stat, int rowIndex,
        out TextMeshProUGUI valueText, out TextMeshProUGUI levelText, out TextMeshProUGUI costText, out Button upgradeBtn)
    {
        float yPos = -rowIndex * 100 - 20;

        GameObject row = new GameObject(statName + "Row");
        row.transform.SetParent(parent);

        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0, 1);
        rowRect.anchorMax = new Vector2(1, 1);
        rowRect.pivot = new Vector2(0.5f, 1);
        rowRect.anchoredPosition = new Vector2(0, yPos);
        rowRect.sizeDelta = new Vector2(-20, 90);

        Image rowBg = row.AddComponent<Image>();
        rowBg.color = new Color(0.1f, 0.07f, 0.09f, 0.9f);

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(row.transform);

        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(0.2f, 0.5f);
        nameRect.pivot = new Vector2(0, 0.5f);
        nameRect.anchoredPosition = new Vector2(15, 0);
        nameRect.sizeDelta = new Vector2(0, 40);

        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = statName;
        nameText.fontSize = 22;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.MidlineLeft;
        nameText.color = new Color(0.85f, 0.75f, 0.6f);

        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(row.transform);

        RectTransform valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.2f, 0.5f);
        valueRect.anchorMax = new Vector2(0.35f, 0.5f);
        valueRect.pivot = new Vector2(0.5f, 0.5f);
        valueRect.anchoredPosition = Vector2.zero;
        valueRect.sizeDelta = new Vector2(0, 40);

        valueText = valueObj.AddComponent<TextMeshProUGUI>();
        valueText.text = "0";
        valueText.fontSize = 26;
        valueText.fontStyle = FontStyles.Bold;
        valueText.alignment = TextAlignmentOptions.Center;
        valueText.color = new Color(0.95f, 0.9f, 0.8f);

        GameObject levelObj = new GameObject("Level");
        levelObj.transform.SetParent(row.transform);

        RectTransform levelRect = levelObj.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.35f, 0.5f);
        levelRect.anchorMax = new Vector2(0.5f, 0.5f);
        levelRect.pivot = new Vector2(0.5f, 0.5f);
        levelRect.anchoredPosition = Vector2.zero;
        levelRect.sizeDelta = new Vector2(0, 30);

        levelText = levelObj.AddComponent<TextMeshProUGUI>();
        levelText.text = "Lv.0";
        levelText.fontSize = 18;
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.color = new Color(0.6f, 0.55f, 0.5f);

        GameObject costObj = new GameObject("Cost");
        costObj.transform.SetParent(row.transform);

        RectTransform costRect = costObj.AddComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0.5f, 0.5f);
        costRect.anchorMax = new Vector2(0.75f, 0.5f);
        costRect.pivot = new Vector2(0.5f, 0.5f);
        costRect.anchoredPosition = Vector2.zero;
        costRect.sizeDelta = new Vector2(0, 30);

        costText = costObj.AddComponent<TextMeshProUGUI>();
        costText.text = "100 DC";
        costText.fontSize = 20;
        costText.fontStyle = FontStyles.Bold;
        costText.alignment = TextAlignmentOptions.Center;
        costText.color = new Color(1f, 0.85f, 0.4f);

        GameObject btnObj = new GameObject("UpgradeButton");
        btnObj.transform.SetParent(row.transform);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.78f, 0.15f);
        btnRect.anchorMax = new Vector2(0.98f, 0.85f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = new Color(0.4f, 0.6f, 0.3f, 1f);

        upgradeBtn = btnObj.AddComponent<Button>();
        UpgradeStat capturedStat = stat;
        upgradeBtn.onClick.AddListener(() => RequestUpgrade(capturedStat));

        ColorBlock colors = upgradeBtn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.2f, 1.1f);
        colors.pressedColor = new Color(0.8f, 0.9f, 0.8f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
        upgradeBtn.colors = colors;

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform);

        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "UPGRADE";
        btnText.fontSize = 16;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
    }

    void RequestUpgrade(UpgradeStat stat)
    {
        UpgradeCost cost = GetUpgradeCost(stat);

        if (currencyManager == null) return;

        bool canAfford = false;
        if (cost.duskenCost > 0)
        {
            canAfford = currencyManager.DuskenCoin >= cost.duskenCost;
        }
        else if (cost.bloodShardCost > 0)
        {
            canAfford = currencyManager.BloodShards >= cost.bloodShardCost;
        }
        else
        {
            canAfford = true;
        }

        if (!canAfford)
        {
            Debug.Log($"[ThrallUpgradeUI] Cannot afford upgrade for {stat}");
            return;
        }

        if (cost.duskenCost > 0)
        {
            currencyManager.SpendDuskenCoin(cost.duskenCost);
        }
        if (cost.bloodShardCost > 0)
        {
            currencyManager.SpendBloodShards(cost.bloodShardCost);
        }

        ApplyUpgrade(stat, cost.statIncrease);

        statLevels[(int)stat]++;
        UpdateStatsDisplay();
        UpdateButtonStates();

        OnUpgradeRequested?.Invoke(stat);
        OnUpgradeCompleted?.Invoke(stat, true);

        Debug.Log($"[ThrallUpgradeUI] Upgraded {stat} to level {statLevels[(int)stat]}");
    }

    void ApplyUpgrade(UpgradeStat stat, float increase)
    {
        if (thrall == null) return;

        CombatStats currentStats = thrall.Stats;

        switch (stat)
        {
            case UpgradeStat.Attack:
                currentStats.attack += increase;
                break;
            case UpgradeStat.Defense:
                currentStats.defense += increase;
                break;
            case UpgradeStat.MaxHp:
                currentStats.maxHealth += increase;
                break;
            case UpgradeStat.Speed:
                currentStats.speed += increase;
                break;
        }

        thrall.Initialize(currentStats);
    }

    public UpgradeCost GetUpgradeCost(UpgradeStat stat)
    {
        int level = statLevels[(int)stat];
        float multiplier = 1f + level * 0.25f;

        UpgradeCost cost = new UpgradeCost
        {
            stat = stat,
            currentLevel = level
        };

        switch (stat)
        {
            case UpgradeStat.Attack:
                cost.duskenCost = Mathf.FloorToInt(100 * multiplier);
                cost.bloodShardCost = 0;
                cost.statIncrease = 5;
                break;
            case UpgradeStat.Defense:
                cost.duskenCost = Mathf.FloorToInt(80 * multiplier);
                cost.bloodShardCost = 0;
                cost.statIncrease = 3;
                break;
            case UpgradeStat.MaxHp:
                cost.duskenCost = Mathf.FloorToInt(120 * multiplier);
                cost.bloodShardCost = 0;
                cost.statIncrease = 20;
                break;
            case UpgradeStat.Speed:
                cost.duskenCost = Mathf.FloorToInt(150 * multiplier);
                cost.bloodShardCost = Mathf.FloorToInt(1 * multiplier);
                cost.statIncrease = 0.1f;
                break;
        }

        return cost;
    }

    void UpdateStatsDisplay()
    {
        if (thrall == null) return;

        if (attackValueText != null)
            attackValueText.text = thrall.Stats.attack.ToString("F0");
        if (defenseValueText != null)
            defenseValueText.text = thrall.Stats.defense.ToString("F0");
        if (maxHpValueText != null)
            maxHpValueText.text = thrall.Stats.maxHealth.ToString("F0");
        if (speedValueText != null)
            speedValueText.text = thrall.Stats.speed.ToString("F2");

        if (attackLevelText != null)
            attackLevelText.text = $"Lv.{statLevels[(int)UpgradeStat.Attack]}";
        if (defenseLevelText != null)
            defenseLevelText.text = $"Lv.{statLevels[(int)UpgradeStat.Defense]}";
        if (maxHpLevelText != null)
            maxHpLevelText.text = $"Lv.{statLevels[(int)UpgradeStat.MaxHp]}";
        if (speedLevelText != null)
            speedLevelText.text = $"Lv.{statLevels[(int)UpgradeStat.Speed]}";

        UpdateCostDisplays();
    }

    void UpdateCostDisplays()
    {
        UpgradeCost attackCost = GetUpgradeCost(UpgradeStat.Attack);
        UpgradeCost defenseCost = GetUpgradeCost(UpgradeStat.Defense);
        UpgradeCost maxHpCost = GetUpgradeCost(UpgradeStat.MaxHp);
        UpgradeCost speedCost = GetUpgradeCost(UpgradeStat.Speed);

        if (attackCostText != null)
            attackCostText.text = $"{attackCost.duskenCost} DC";
        if (defenseCostText != null)
            defenseCostText.text = $"{defenseCost.duskenCost} DC";
        if (maxHpCostText != null)
            maxHpCostText.text = $"{maxHpCost.duskenCost} DC";
        if (speedCostText != null)
        {
            if (speedCost.bloodShardCost > 0)
                speedCostText.text = $"{speedCost.duskenCost} DC + {speedCost.bloodShardCost} BS";
            else
                speedCostText.text = $"{speedCost.duskenCost} DC";
        }
    }

    void UpdateButtonStates()
    {
        if (currencyManager == null) return;

        int dusken = currencyManager.DuskenCoin;
        int shards = currencyManager.BloodShards;

        if (attackUpgradeBtn != null)
            attackUpgradeBtn.interactable = dusken >= GetUpgradeCost(UpgradeStat.Attack).duskenCost;
        if (defenseUpgradeBtn != null)
            defenseUpgradeBtn.interactable = dusken >= GetUpgradeCost(UpgradeStat.Defense).duskenCost;
        if (maxHpUpgradeBtn != null)
            maxHpUpgradeBtn.interactable = dusken >= GetUpgradeCost(UpgradeStat.MaxHp).duskenCost;
        if (speedUpgradeBtn != null)
        {
            UpgradeCost speedCost = GetUpgradeCost(UpgradeStat.Speed);
            speedUpgradeBtn.interactable = dusken >= speedCost.duskenCost && shards >= speedCost.bloodShardCost;
        }
    }

    public int GetStatLevel(UpgradeStat stat)
    {
        return statLevels[(int)stat];
    }

    void OnDestroy()
    {
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged -= OnCurrencyChanged;
        }
    }
}

