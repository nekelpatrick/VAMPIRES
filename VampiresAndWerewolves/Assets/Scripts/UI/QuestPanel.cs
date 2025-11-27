using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestPanel : MonoBehaviour, IThemeable
{
    public static QuestPanel Instance { get; private set; }

    private UITheme Theme => UIThemeManager.Theme;

    [Header("Panel Settings")]
    private Canvas panelCanvas;
    private GameObject panelRoot;
    private Image panelBackground;
    private RectTransform questContainer;
    private TextMeshProUGUI titleText;
    private Button closeButton;
    private bool isOpen;

    private List<QuestRowUI> questRows = new List<QuestRowUI>();
    private DailyQuestManager questManager;
    private AdRewardManager adManager;

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

    public void ApplyTheme(UITheme theme)
    {
        if (theme == null) return;

        if (panelBackground != null)
            panelBackground.color = theme.backgroundPanel;

        if (titleText != null)
            titleText.color = theme.textGold;

        foreach (var row in questRows)
        {
            row.ApplyTheme(theme);
        }
    }

    void Start()
    {
        questManager = DailyQuestManager.Instance;
        adManager = AdRewardManager.Instance;

        CreatePanel();
        UIThemeManager.Instance?.RegisterElement(this);
        HidePanel();

        if (questManager != null)
        {
            questManager.OnQuestProgress += RefreshQuest;
            questManager.OnQuestCompleted += OnQuestCompleted;
            questManager.OnQuestClaimed += RefreshQuest;
            questManager.OnQuestsReset += RefreshAllQuests;
        }
    }

    void CreatePanel()
    {
        bool isMobile = MobileUIScaler.Instance != null && MobileUIScaler.Instance.IsMobile;

        panelCanvas = UIFactory.CreateCanvas(transform, "QuestPanelCanvas", 400);

        float panelWidth = isMobile ? 500 : 420;
        float panelHeight = isMobile ? 700 : 600;

        panelRoot = new GameObject("PanelRoot");
        panelRoot.transform.SetParent(panelCanvas.transform);

        RectTransform panelRect = panelRoot.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.pivot = new Vector2(1, 0.5f);
        panelRect.anchoredPosition = new Vector2(-25, 0);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        panelBackground = panelRoot.AddComponent<Image>();
        panelBackground.color = Theme.backgroundPanel;

        UIFactory.AddBorder(panelRoot.transform, Theme.borderGold);

        CreateHeader(panelRoot.transform);
        CreateQuestContainer(panelRoot.transform);
        CreateCloseButton(panelRoot.transform);
    }

    void CreateHeader(Transform parent)
    {
        GameObject header = new GameObject("Header");
        header.transform.SetParent(parent);

        RectTransform rect = header.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -Theme.spacingMD);
        rect.sizeDelta = new Vector2(-Theme.spacingXL, 60);

        titleText = header.AddComponent<TextMeshProUGUI>();
        titleText.text = "DAILY QUESTS";
        titleText.fontSize = Theme.GetScaledFontSize(Theme.fontSizeLG);
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Theme.textGold;
    }

    void CreateQuestContainer(Transform parent)
    {
        GameObject container = new GameObject("QuestContainer");
        container.transform.SetParent(parent);

        questContainer = container.AddComponent<RectTransform>();
        questContainer.anchorMin = new Vector2(0, 0);
        questContainer.anchorMax = new Vector2(1, 1);
        questContainer.offsetMin = new Vector2(Theme.spacingMD, 60);
        questContainer.offsetMax = new Vector2(-Theme.spacingMD, -80);

        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = Theme.spacingSM;
        layout.padding = new RectOffset(
            (int)Theme.spacingXS, 
            (int)Theme.spacingXS, 
            (int)Theme.spacingXS, 
            (int)Theme.spacingXS
        );
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    void CreateCloseButton(Transform parent)
    {
        float btnSize = Theme.GetScaledSize(Theme.buttonSizeSmall);

        closeButton = UIFactory.CreateIconButton(parent, "CloseButton", "X", btnSize, HidePanel);

        RectTransform rect = closeButton.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-Theme.spacingSM, -Theme.spacingSM);

        Image btnImage = closeButton.GetComponent<Image>();
        btnImage.color = Theme.borderBlood;
    }

    public void ShowPanel()
    {
        isOpen = true;
        panelRoot.SetActive(true);
        RefreshAllQuests();
    }

    public void HidePanel()
    {
        isOpen = false;
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Toggle()
    {
        if (isOpen)
            HidePanel();
        else
            ShowPanel();
    }

    void RefreshAllQuests()
    {
        foreach (var row in questRows)
        {
            if (row != null && row.gameObject != null)
            {
                Destroy(row.gameObject);
            }
        }
        questRows.Clear();

        if (questManager == null) return;

        List<QuestProgress> quests = questManager.GetDailyQuests();
        foreach (var quest in quests)
        {
            CreateQuestRow(quest);
        }
    }

    void RefreshQuest(QuestProgress quest)
    {
        foreach (var row in questRows)
        {
            if (row.questId == quest.questId)
            {
                row.UpdateProgress(quest);
                return;
            }
        }
    }

    void OnQuestCompleted(QuestProgress quest)
    {
        RefreshQuest(quest);
        QuestNotification.Instance?.ShowCompletion(quest);
    }

    void CreateQuestRow(QuestProgress quest)
    {
        GameObject rowObj = new GameObject($"Quest_{quest.questId}");
        rowObj.transform.SetParent(questContainer);

        RectTransform rect = rowObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 90);

        Image bg = rowObj.AddComponent<Image>();
        bg.color = Theme.backgroundSecondary;

        QuestRowUI rowUI = rowObj.AddComponent<QuestRowUI>();
        rowUI.Initialize(quest, OnClaimClicked, Theme);

        questRows.Add(rowUI);
    }

    void OnClaimClicked(QuestProgress quest)
    {
        if (adManager != null && adManager.CanOfferAd(AdType.DoubleLoot))
        {
            ShowClaimWithAdOption(quest);
        }
        else
        {
            questManager?.ClaimQuest(quest.questId, false);
        }
    }

    void ShowClaimWithAdOption(QuestProgress quest)
    {
        AdPromptPanel.Instance?.ShowPrompt(AdType.DoubleLoot);
    }

    void OnDestroy()
    {
        UIThemeManager.Instance?.UnregisterElement(this);

        if (questManager != null)
        {
            questManager.OnQuestProgress -= RefreshQuest;
            questManager.OnQuestCompleted -= OnQuestCompleted;
            questManager.OnQuestClaimed -= RefreshQuest;
            questManager.OnQuestsReset -= RefreshAllQuests;
        }
    }
}

public class QuestRowUI : MonoBehaviour
{
    public string questId;

    private TextMeshProUGUI nameText;
    private TextMeshProUGUI progressText;
    private TextMeshProUGUI rewardText;
    private Image progressFill;
    private Image progressBackground;
    private Image rowBackground;
    private Button claimButton;
    private Image claimButtonImage;
    private TextMeshProUGUI claimButtonText;

    private System.Action<QuestProgress> onClaimClicked;
    private QuestProgress currentQuest;
    private UITheme theme;

    public void Initialize(QuestProgress quest, System.Action<QuestProgress> claimCallback, UITheme initialTheme)
    {
        questId = quest.questId;
        currentQuest = quest;
        onClaimClicked = claimCallback;
        theme = initialTheme ?? UIThemeManager.Theme;

        rowBackground = GetComponent<Image>();
        CreateUI();
        UpdateProgress(quest);
    }

    public void ApplyTheme(UITheme newTheme)
    {
        theme = newTheme;

        if (rowBackground != null)
            rowBackground.color = theme.backgroundSecondary;

        if (nameText != null)
            nameText.color = theme.textSecondary;

        if (progressText != null)
            progressText.color = theme.textMuted;

        if (progressBackground != null)
            progressBackground.color = theme.backgroundDark;

        if (progressFill != null)
            progressFill.color = theme.fillProgress;

        if (rewardText != null)
            rewardText.color = theme.textGold;
    }

    void CreateUI()
    {
        nameText = UIFactory.CreateText(transform, "Name", currentQuest.questName,
            theme.GetScaledFontSize(theme.fontSizeSM), theme.textSecondary, FontStyles.Normal, 200);
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.anchoredPosition = new Vector2(theme.spacingSM, -theme.spacingSM);
        nameRect.sizeDelta = new Vector2(-100, 30);
        nameText.alignment = TextAlignmentOptions.Left;

        progressText = UIFactory.CreateText(transform, "Progress", "0/100",
            theme.GetScaledFontSize(theme.fontSizeXS), theme.textMuted, FontStyles.Normal, 80);
        RectTransform progressRect = progressText.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(1, 1);
        progressRect.anchorMax = new Vector2(1, 1);
        progressRect.pivot = new Vector2(1, 1);
        progressRect.anchoredPosition = new Vector2(-theme.spacingSM, -theme.spacingMD);
        progressRect.sizeDelta = new Vector2(80, 25);
        progressText.alignment = TextAlignmentOptions.Right;

        GameObject barBg = new GameObject("BarBg");
        barBg.transform.SetParent(transform);
        RectTransform barBgRect = barBg.AddComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0, 0.5f);
        barBgRect.anchorMax = new Vector2(1, 0.5f);
        barBgRect.pivot = new Vector2(0, 0.5f);
        barBgRect.anchoredPosition = new Vector2(theme.spacingSM, 0);
        barBgRect.sizeDelta = new Vector2(-120, 16);
        progressBackground = barBg.AddComponent<Image>();
        progressBackground.color = theme.backgroundDark;

        GameObject barFill = new GameObject("BarFill");
        barFill.transform.SetParent(barBg.transform);
        RectTransform fillRect = barFill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.offsetMin = new Vector2(theme.barInnerPadding, theme.barInnerPadding);
        fillRect.offsetMax = new Vector2(-theme.barInnerPadding, -theme.barInnerPadding);
        progressFill = barFill.AddComponent<Image>();
        progressFill.color = theme.fillProgress;

        rewardText = UIFactory.CreateText(transform, "Reward", "+500 Dusken",
            theme.GetScaledFontSize(theme.fontSizeXS), theme.textGold, FontStyles.Bold, 150);
        RectTransform rewardRect = rewardText.GetComponent<RectTransform>();
        rewardRect.anchorMin = new Vector2(0, 0);
        rewardRect.anchorMax = new Vector2(0, 0);
        rewardRect.pivot = new Vector2(0, 0);
        rewardRect.anchoredPosition = new Vector2(theme.spacingSM, theme.spacingMD);
        rewardRect.sizeDelta = new Vector2(150, 25);
        rewardText.alignment = TextAlignmentOptions.Left;

        GameObject btnObj = new GameObject("ClaimBtn");
        btnObj.transform.SetParent(transform);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0);
        btnRect.anchorMax = new Vector2(1, 0);
        btnRect.pivot = new Vector2(1, 0);
        btnRect.anchoredPosition = new Vector2(-theme.spacingSM, theme.spacingMD);
        btnRect.sizeDelta = new Vector2(90, 35);

        claimButtonImage = btnObj.AddComponent<Image>();
        claimButtonImage.color = theme.statusActive;

        claimButton = btnObj.AddComponent<Button>();
        claimButton.onClick.AddListener(() => onClaimClicked?.Invoke(currentQuest));

        ColorBlock colors = claimButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
        claimButton.colors = colors;

        claimButtonText = UIFactory.CreateText(btnObj.transform, "BtnText", "CLAIM",
            theme.GetScaledFontSize(theme.fontSizeXS), theme.textPrimary, FontStyles.Bold);
        RectTransform btnTextRect = claimButtonText.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        claimButtonText.alignment = TextAlignmentOptions.Center;
    }

    public void UpdateProgress(QuestProgress quest)
    {
        currentQuest = quest;

        if (nameText != null)
            nameText.text = quest.questName;

        if (progressText != null)
            progressText.text = $"{quest.currentValue}/{quest.targetValue}";

        if (progressFill != null)
        {
            RectTransform rect = progressFill.GetComponent<RectTransform>();
            float progress = (float)quest.currentValue / quest.targetValue;
            rect.anchorMax = new Vector2(Mathf.Clamp01(progress), 1);
        }

        if (rewardText != null)
        {
            if (quest.bloodShardsReward > 0)
                rewardText.text = $"+{quest.bloodShardsReward} Shards";
            else
                rewardText.text = $"+{quest.duskenReward} Dusken";
        }

        if (claimButton != null)
        {
            claimButton.gameObject.SetActive(quest.isCompleted && !quest.isClaimed);

            if (quest.isClaimed)
            {
                claimButtonText.text = "DONE";
                claimButtonImage.color = theme.buttonDisabled;
            }
        }
    }
}
