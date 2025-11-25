using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestPanel : MonoBehaviour
{
    public static QuestPanel Instance { get; private set; }

    [Header("Panel Settings")]
    private GameObject panelRoot;
    private RectTransform questContainer;
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

    void Start()
    {
        questManager = DailyQuestManager.Instance;
        adManager = AdRewardManager.Instance;

        CreatePanel();
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
        GameObject canvasObj = new GameObject("QuestPanelCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        panelRoot = new GameObject("PanelRoot");
        panelRoot.transform.SetParent(canvasObj.transform);

        RectTransform panelRect = panelRoot.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.pivot = new Vector2(1, 0.5f);
        panelRect.anchoredPosition = new Vector2(-20, 0);
        panelRect.sizeDelta = new Vector2(420, 600);

        Image panelBg = panelRoot.AddComponent<Image>();
        panelBg.color = new Color(0.06f, 0.04f, 0.08f, 0.95f);

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
        rect.anchoredPosition = new Vector2(0, -10);
        rect.sizeDelta = new Vector2(-40, 60);

        TextMeshProUGUI title = header.AddComponent<TextMeshProUGUI>();
        title.text = "DAILY QUESTS";
        title.fontSize = 32;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(1f, 0.85f, 0.3f);
    }

    void CreateQuestContainer(Transform parent)
    {
        GameObject container = new GameObject("QuestContainer");
        container.transform.SetParent(parent);

        questContainer = container.AddComponent<RectTransform>();
        questContainer.anchorMin = new Vector2(0, 0);
        questContainer.anchorMax = new Vector2(1, 1);
        questContainer.offsetMin = new Vector2(15, 60);
        questContainer.offsetMax = new Vector2(-15, -80);

        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    void CreateCloseButton(Transform parent)
    {
        GameObject btnObj = new GameObject("CloseButton");
        btnObj.transform.SetParent(parent);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-10, -10);
        rect.sizeDelta = new Vector2(40, 40);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.6f, 0.2f, 0.2f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(HidePanel);

        GameObject textObj = new GameObject("X");
        textObj.transform.SetParent(btnObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "X";
        text.fontSize = 24;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
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
            if (row.gameObject != null)
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
        bg.color = new Color(0.1f, 0.08f, 0.12f, 0.9f);

        QuestRowUI rowUI = rowObj.AddComponent<QuestRowUI>();
        rowUI.Initialize(quest, OnClaimClicked);

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
    private Button claimButton;
    private TextMeshProUGUI claimButtonText;

    private System.Action<QuestProgress> onClaimClicked;
    private QuestProgress currentQuest;

    public void Initialize(QuestProgress quest, System.Action<QuestProgress> claimCallback)
    {
        questId = quest.questId;
        currentQuest = quest;
        onClaimClicked = claimCallback;

        CreateUI();
        UpdateProgress(quest);
    }

    void CreateUI()
    {
        nameText = CreateText("Name", currentQuest.questName, 20,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -10), new Vector2(-100, 30));
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.color = new Color(0.9f, 0.85f, 0.75f);

        progressText = CreateText("Progress", "0/100", 16,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -15), new Vector2(80, 25));
        progressText.alignment = TextAlignmentOptions.Right;
        progressText.color = new Color(0.7f, 0.7f, 0.7f);

        GameObject barBg = CreateImage("BarBg", new Color(0.2f, 0.15f, 0.2f),
            new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(10, 0), new Vector2(-120, 16));

        GameObject barFill = CreateImage("BarFill", new Color(0.3f, 0.8f, 0.4f),
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        barFill.transform.SetParent(barBg.transform);
        RectTransform fillRect = barFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        progressFill = barFill.GetComponent<Image>();

        rewardText = CreateText("Reward", "+500 Dusken", 16,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(10, 15), new Vector2(150, 25));
        rewardText.alignment = TextAlignmentOptions.Left;
        rewardText.color = new Color(1f, 0.85f, 0.3f);

        GameObject btnObj = CreateImage("ClaimBtn", new Color(0.2f, 0.6f, 0.3f),
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-10, 15), new Vector2(90, 35));

        claimButton = btnObj.AddComponent<Button>();
        claimButton.onClick.AddListener(() => onClaimClicked?.Invoke(currentQuest));

        GameObject btnText = new GameObject("BtnText");
        btnText.transform.SetParent(btnObj.transform);

        RectTransform btnTextRect = btnText.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        claimButtonText = btnText.AddComponent<TextMeshProUGUI>();
        claimButtonText.text = "CLAIM";
        claimButtonText.fontSize = 16;
        claimButtonText.fontStyle = FontStyles.Bold;
        claimButtonText.alignment = TextAlignmentOptions.Center;
        claimButtonText.color = Color.white;
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
                claimButton.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            }
        }
    }

    TextMeshProUGUI CreateText(string name, string content, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;

        return text;
    }

    GameObject CreateImage(string name, Color color, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;

        return obj;
    }
}

