using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GothicHUD : MonoBehaviour
{
    public static GothicHUD Instance { get; private set; }

    public event Action<int> OnComboChanged;

    [Header("References")]
    private TextMeshProUGUI waveText;
    private TextMeshProUGUI duskenText;
    private TextMeshProUGUI shardsText;
    private TextMeshProUGUI killCountText;
    private TextMeshProUGUI comboText;
    private Image thrallHealthFill;
    private Image waveProgressFill;
    private Image comboFill;

    [Header("Ad & Quest UI")]
    private Button damageBoostButton;
    private Image damageBoostGlow;
    private TextMeshProUGUI boostTimerText;
    private Button questButton;
    private TextMeshProUGUI questBadge;

    [Header("Shop & Subscription")]
    private Button shopButton;
    private ShopPanel shopPanel;
    private SubscriptionBanner subscriptionBanner;

    private CurrencyManager currencyManager;
    private HordeSpawner hordeSpawner;
    private CombatManager combatManager;
    private ThrallController thrall;
    private AdRewardManager adManager;
    private DailyQuestManager questManager;

    private int killCount;
    private int comboCount;
    private float comboTimer;
    private float maxComboTime = 3f;
    private int currentWave = 1;
    private int enemiesInWave;
    private int enemiesKilledInWave;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CreateHUD();

        currencyManager = CurrencyManager.Instance;
        combatManager = CombatManager.Instance;
        hordeSpawner = UnityEngine.Object.FindFirstObjectByType<HordeSpawner>();

        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged += UpdateCurrency;
            UpdateCurrency(currencyManager.DuskenCoin, currencyManager.BloodShards);
        }

        if (hordeSpawner != null)
        {
            hordeSpawner.OnWaveStarted += OnWaveStart;
        }

        if (combatManager != null)
        {
            thrall = combatManager.GetThrall();
        }

        StartCoroutine(BindAdAndQuestManagers());
    }

    System.Collections.IEnumerator BindAdAndQuestManagers()
    {
        int attempts = 0;
        while (adManager == null && attempts < 10)
        {
            adManager = AdRewardManager.Instance;
            if (adManager == null)
            {
                yield return new WaitForSeconds(0.1f);
                attempts++;
            }
        }

        if (adManager != null)
        {
            adManager.OnDamageBoostStarted += OnDamageBoostStarted;
            adManager.OnDamageBoostEnded += OnDamageBoostEnded;
            Debug.Log("[GothicHUD] AdRewardManager bound successfully");
        }
        else
        {
            Debug.LogWarning("[GothicHUD] AdRewardManager not found after retries");
        }

        attempts = 0;
        while (questManager == null && attempts < 10)
        {
            questManager = DailyQuestManager.Instance;
            if (questManager == null)
            {
                yield return new WaitForSeconds(0.1f);
                attempts++;
            }
        }

        if (questManager != null)
        {
            questManager.OnQuestCompleted += OnQuestCompleted;
            questManager.OnQuestClaimed += OnQuestStatusChanged;
            UpdateQuestBadge();
            Debug.Log("[GothicHUD] DailyQuestManager bound successfully");
        }
        else
        {
            Debug.LogWarning("[GothicHUD] DailyQuestManager not found after retries");
        }
    }

    void OnDamageBoostClicked()
    {
        Debug.Log("[GothicHUD] Damage boost button clicked!");

        if (adManager == null)
        {
            adManager = AdRewardManager.Instance;
        }

        if (adManager != null)
        {
            adManager.RequestDamageBoost((success) =>
            {
                if (success)
                {
                    ScreenEffects.Instance?.FlashGold(0.3f);
                }
            });
        }
        else
        {
            Debug.LogWarning("[GothicHUD] AdRewardManager not available");
        }
    }

    void OnQuestButtonClicked()
    {
        Debug.Log("[GothicHUD] Quest button clicked!");

        QuestPanel panel = QuestPanel.Instance;
        if (panel != null)
        {
            panel.Toggle();
        }
        else
        {
            Debug.LogWarning("[GothicHUD] QuestPanel not available");
        }
    }

    void OnDamageBoostStarted()
    {
        if (damageBoostButton != null)
        {
            damageBoostButton.interactable = false;
            damageBoostButton.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 0.9f);
        }
    }

    void OnDamageBoostEnded()
    {
        if (damageBoostButton != null)
        {
            damageBoostButton.interactable = true;
            damageBoostButton.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f, 0.9f);
        }
        if (boostTimerText != null)
        {
            boostTimerText.text = "BOOST";
        }
    }

    void OnQuestCompleted(QuestProgress quest)
    {
        UpdateQuestBadge();
    }

    void OnQuestStatusChanged(QuestProgress quest)
    {
        UpdateQuestBadge();
    }

    void UpdateQuestBadge()
    {
        if (questBadge == null || questManager == null) return;

        int unclaimed = questManager.GetUnclaimedCount();
        questBadge.transform.parent.gameObject.SetActive(unclaimed > 0);
        questBadge.text = unclaimed.ToString();
    }

    void CreateHUD()
    {
        Canvas existingCanvas = GetComponentInChildren<Canvas>();
        if (existingCanvas != null) return;

        GameObject canvasObj = new GameObject("GothicHUD_Canvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        CreateTopBar(canvasObj.transform);
        CreateBottomBar(canvasObj.transform);
        CreateKillCounter(canvasObj.transform);
        CreateComboMeter(canvasObj.transform);
        CreateAdButtons(canvasObj.transform);
        CreateQuestButton(canvasObj.transform);
        CreateShopButton(canvasObj.transform);
        CreateSubscriptionBanner(canvasObj.transform);
    }

    void CreateAdButtons(Transform parent)
    {
        float buttonSize = MobileUIScaler.Instance != null ? MobileUIScaler.Instance.GetButtonSize(120f) : 120f;
        float fontSize = MobileUIScaler.Instance != null ? MobileUIScaler.Instance.GetFontSize(40f) : 40f;

        GameObject boostBtn = new GameObject("DamageBoostButton");
        boostBtn.transform.SetParent(parent);

        RectTransform btnRect = boostBtn.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0.5f);
        btnRect.anchorMax = new Vector2(1, 0.5f);
        btnRect.pivot = new Vector2(1, 0.5f);
        btnRect.anchoredPosition = new Vector2(-25, 140);
        btnRect.sizeDelta = new Vector2(buttonSize, buttonSize);

        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(boostBtn.transform);
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-4, -4);
        borderRect.offsetMax = new Vector2(4, 4);
        borderObj.transform.SetAsFirstSibling();

        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(0.85f, 0.65f, 0.2f, 1f);
        borderImg.raycastTarget = false;

        Image btnBg = boostBtn.AddComponent<Image>();
        btnBg.color = new Color(0.5f, 0.15f, 0.15f, 0.95f);
        btnBg.raycastTarget = true;

        damageBoostButton = boostBtn.AddComponent<Button>();
        damageBoostButton.onClick.AddListener(OnDamageBoostClicked);

        ColorBlock colors = damageBoostButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        damageBoostButton.colors = colors;

        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(boostBtn.transform);
        RectTransform glowRect = glowObj.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = new Vector2(-15, -15);
        glowRect.offsetMax = new Vector2(15, 15);
        glowObj.transform.SetAsFirstSibling();

        damageBoostGlow = glowObj.AddComponent<Image>();
        damageBoostGlow.color = new Color(1f, 0.3f, 0.3f, 0.5f);
        damageBoostGlow.raycastTarget = false;

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(boostBtn.transform);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(8, 15);
        iconRect.offsetMax = new Vector2(-8, -15);

        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "2X";
        iconText.fontSize = fontSize;
        iconText.fontStyle = FontStyles.Bold;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.color = Color.white;
        iconText.enableAutoSizing = true;
        iconText.fontSizeMin = 24;
        iconText.fontSizeMax = fontSize;
        iconText.raycastTarget = false;

        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(iconObj.transform);
        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(3, -3);
        shadowRect.offsetMax = new Vector2(3, -3);
        shadowRect.anchoredPosition = new Vector2(2, -2);
        shadowObj.transform.SetAsFirstSibling();

        TextMeshProUGUI shadowText = shadowObj.AddComponent<TextMeshProUGUI>();
        shadowText.text = "2X";
        shadowText.fontSize = fontSize;
        shadowText.fontStyle = FontStyles.Bold;
        shadowText.alignment = TextAlignmentOptions.Center;
        shadowText.color = new Color(0, 0, 0, 0.6f);
        shadowText.enableAutoSizing = true;
        shadowText.fontSizeMin = 24;
        shadowText.fontSizeMax = fontSize;
        shadowText.raycastTarget = false;

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(boostBtn.transform);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0);
        labelRect.anchorMax = new Vector2(0.5f, 0);
        labelRect.pivot = new Vector2(0.5f, 1);
        labelRect.anchoredPosition = new Vector2(0, -8);
        labelRect.sizeDelta = new Vector2(140, 30);

        boostTimerText = labelObj.AddComponent<TextMeshProUGUI>();
        boostTimerText.text = "BOOST";
        boostTimerText.fontSize = 18;
        boostTimerText.fontStyle = FontStyles.Bold;
        boostTimerText.alignment = TextAlignmentOptions.Center;
        boostTimerText.color = new Color(1f, 0.85f, 0.3f);
        boostTimerText.raycastTarget = false;
    }

    void CreateQuestButton(Transform parent)
    {
        float buttonSize = MobileUIScaler.Instance != null ? MobileUIScaler.Instance.GetButtonSize(120f) : 120f;
        float fontSize = MobileUIScaler.Instance != null ? MobileUIScaler.Instance.GetFontSize(48f) : 48f;

        GameObject questBtn = new GameObject("QuestButton");
        questBtn.transform.SetParent(parent);

        RectTransform btnRect = questBtn.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0.5f);
        btnRect.anchorMax = new Vector2(1, 0.5f);
        btnRect.pivot = new Vector2(1, 0.5f);
        btnRect.anchoredPosition = new Vector2(-25, 0);
        btnRect.sizeDelta = new Vector2(buttonSize, buttonSize);

        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(questBtn.transform);
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-4, -4);
        borderRect.offsetMax = new Vector2(4, 4);
        borderObj.transform.SetAsFirstSibling();

        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(0.85f, 0.65f, 0.2f, 1f);
        borderImg.raycastTarget = false;

        Image btnBg = questBtn.AddComponent<Image>();
        btnBg.color = new Color(0.2f, 0.4f, 0.15f, 0.95f);
        btnBg.raycastTarget = true;

        questButton = questBtn.AddComponent<Button>();
        questButton.onClick.AddListener(OnQuestButtonClicked);

        ColorBlock colors = questButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        questButton.colors = colors;

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(questBtn.transform);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(8, 8);
        iconRect.offsetMax = new Vector2(-8, -8);

        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(iconObj.transform);
        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(3, -3);
        shadowRect.offsetMax = new Vector2(3, -3);
        shadowRect.anchoredPosition = new Vector2(2, -2);
        shadowObj.transform.SetAsFirstSibling();

        TextMeshProUGUI shadowText = shadowObj.AddComponent<TextMeshProUGUI>();
        shadowText.text = "Q";
        shadowText.fontSize = fontSize;
        shadowText.fontStyle = FontStyles.Bold;
        shadowText.alignment = TextAlignmentOptions.Center;
        shadowText.color = new Color(0, 0, 0, 0.6f);
        shadowText.enableAutoSizing = true;
        shadowText.fontSizeMin = 28;
        shadowText.fontSizeMax = fontSize;
        shadowText.raycastTarget = false;

        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "Q";
        iconText.fontSize = fontSize;
        iconText.fontStyle = FontStyles.Bold;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.color = Color.white;
        iconText.enableAutoSizing = true;
        iconText.fontSizeMin = 28;
        iconText.fontSizeMax = fontSize;
        iconText.raycastTarget = false;

        GameObject badgeObj = new GameObject("Badge");
        badgeObj.transform.SetParent(questBtn.transform);

        RectTransform badgeRect = badgeObj.AddComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(1, 1);
        badgeRect.anchorMax = new Vector2(1, 1);
        badgeRect.pivot = new Vector2(0.5f, 0.5f);
        badgeRect.anchoredPosition = new Vector2(-8, -8);
        badgeRect.sizeDelta = new Vector2(36, 36);

        Image badgeBg = badgeObj.AddComponent<Image>();
        badgeBg.color = new Color(0.9f, 0.2f, 0.2f);
        badgeBg.raycastTarget = false;

        GameObject badgeText = new GameObject("Text");
        badgeText.transform.SetParent(badgeObj.transform);

        RectTransform textRect = badgeText.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        questBadge = badgeText.AddComponent<TextMeshProUGUI>();
        questBadge.text = "0";
        questBadge.fontSize = 22;
        questBadge.fontStyle = FontStyles.Bold;
        questBadge.alignment = TextAlignmentOptions.Center;
        questBadge.color = Color.white;
        questBadge.raycastTarget = false;

        badgeObj.SetActive(false);
    }

    void CreateShopButton(Transform parent)
    {
        float buttonSize = MobileUIScaler.Instance != null ? MobileUIScaler.Instance.GetButtonSize(120f) : 120f;
        float fontSize = MobileUIScaler.Instance != null ? MobileUIScaler.Instance.GetFontSize(48f) : 48f;

        GameObject shopBtn = new GameObject("ShopButton");
        shopBtn.transform.SetParent(parent);

        RectTransform btnRect = shopBtn.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0.5f);
        btnRect.anchorMax = new Vector2(1, 0.5f);
        btnRect.pivot = new Vector2(1, 0.5f);
        btnRect.anchoredPosition = new Vector2(-25, -140);
        btnRect.sizeDelta = new Vector2(buttonSize, buttonSize);

        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(shopBtn.transform);
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-4, -4);
        borderRect.offsetMax = new Vector2(4, 4);
        borderObj.transform.SetAsFirstSibling();

        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(0.85f, 0.65f, 0.2f, 1f);
        borderImg.raycastTarget = false;

        Image btnBg = shopBtn.AddComponent<Image>();
        btnBg.color = new Color(0.5f, 0.35f, 0.15f, 0.95f);
        btnBg.raycastTarget = true;

        shopButton = shopBtn.AddComponent<Button>();
        shopButton.onClick.AddListener(OnShopButtonClicked);

        ColorBlock colors = shopButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        shopButton.colors = colors;

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(shopBtn.transform);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(8, 8);
        iconRect.offsetMax = new Vector2(-8, -8);

        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(iconObj.transform);
        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(2, -2);
        shadowRect.offsetMax = new Vector2(2, -2);
        shadowObj.transform.SetAsFirstSibling();

        TextMeshProUGUI shadowText = shadowObj.AddComponent<TextMeshProUGUI>();
        shadowText.text = "S";
        shadowText.fontSize = fontSize;
        shadowText.fontStyle = FontStyles.Bold;
        shadowText.alignment = TextAlignmentOptions.Center;
        shadowText.color = new Color(0, 0, 0, 0.6f);
        shadowText.enableAutoSizing = true;
        shadowText.fontSizeMin = 28;
        shadowText.fontSizeMax = fontSize;
        shadowText.raycastTarget = false;

        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "S";
        iconText.fontSize = fontSize;
        iconText.fontStyle = FontStyles.Bold;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.color = new Color(1f, 0.85f, 0.4f);
        iconText.enableAutoSizing = true;
        iconText.fontSizeMin = 28;
        iconText.fontSizeMax = fontSize;
        iconText.raycastTarget = false;

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(shopBtn.transform);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0);
        labelRect.anchorMax = new Vector2(0.5f, 0);
        labelRect.pivot = new Vector2(0.5f, 1);
        labelRect.anchoredPosition = new Vector2(0, -8);
        labelRect.sizeDelta = new Vector2(140, 30);

        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = "SHOP";
        labelText.fontSize = 18;
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = new Color(1f, 0.85f, 0.3f);
        labelText.raycastTarget = false;
    }

    void OnShopButtonClicked()
    {
        Debug.Log("[GothicHUD] Shop button clicked!");

        if (shopPanel == null)
        {
            shopPanel = ShopPanel.Instance;
        }

        if (shopPanel == null)
        {
            GameObject shopObj = new GameObject("ShopPanel");
            shopObj.transform.SetParent(transform);
            shopPanel = shopObj.AddComponent<ShopPanel>();
            shopPanel.Initialize();
            Debug.Log("[GothicHUD] ShopPanel created and initialized");
        }

        if (shopPanel != null)
        {
            shopPanel.Toggle();
        }
        else
        {
            Debug.LogWarning("[GothicHUD] ShopPanel not available");
        }
    }

    void CreateSubscriptionBanner(Transform parent)
    {
        if (subscriptionBanner == null)
        {
            GameObject bannerObj = new GameObject("SubscriptionBanner");
            bannerObj.transform.SetParent(parent);
            subscriptionBanner = bannerObj.AddComponent<SubscriptionBanner>();
        }

        subscriptionBanner.CreateBanner(parent);
        subscriptionBanner.OnSubscribeClicked += OnSubscribeBannerClicked;
    }

    void OnSubscribeBannerClicked()
    {
        Debug.Log("[GothicHUD] Subscription banner clicked!");

        if (shopPanel == null)
        {
            shopPanel = ShopPanel.Instance;
        }

        if (shopPanel != null)
        {
            shopPanel.Show();
        }
    }

    void CreateTopBar(Transform parent)
    {
        GameObject topBar = CreatePanel(parent, "TopBar",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(0, -10), new Vector2(-40, 70));

        AddGothicBorder(topBar.transform, new Color(0.6f, 0.4f, 0.2f, 0.8f));

        Image bg = topBar.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.03f, 0.05f, 0.95f);

        HorizontalLayoutGroup layout = topBar.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 50;
        layout.padding = new RectOffset(40, 40, 10, 10);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;

        waveText = CreateGothicText(topBar.transform, "WaveText", "WAVE I", 36,
            new Color(0.9f, 0.8f, 0.6f), FontStyles.Bold, 200);

        CreateCurrencyDisplay(topBar.transform, "DuskenDisplay", "[DUSKEN COIN]",
            new Color(1f, 0.85f, 0.4f), out duskenText);

        CreateCurrencyDisplay(topBar.transform, "ShardsDisplay", "[BLOOD SHARDS]",
            new Color(0.9f, 0.2f, 0.25f), out shardsText);

        thrallHealthFill = CreateHealthBar(topBar.transform);
    }

    void CreateCurrencyDisplay(Transform parent, string name, string label, Color color, out TextMeshProUGUI valueText)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent);

        RectTransform rect = container.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220, 50);

        HorizontalLayoutGroup hLayout = container.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 8;
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = true;

        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(container.transform);
        RectTransform iconRect = icon.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(30, 30);

        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = color;

        CreateGothicText(container.transform, "Label", label.Split(' ')[0].Replace("[", "").Replace("]", "") + ":",
            16, new Color(0.6f, 0.55f, 0.5f), FontStyles.Normal, 80);

        valueText = CreateGothicText(container.transform, "Value", "0",
            24, color, FontStyles.Bold, 100);
    }

    Image CreateHealthBar(Transform parent)
    {
        GameObject container = new GameObject("ThrallHealth");
        container.transform.SetParent(parent);

        RectTransform rect = container.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 40);

        CreateGothicText(container.transform, "Label", "THRALL", 14,
            new Color(0.5f, 0.7f, 0.5f), FontStyles.Normal, 60);

        GameObject barBg = new GameObject("HealthBarBg");
        barBg.transform.SetParent(container.transform);
        RectTransform bgRect = barBg.AddComponent<RectTransform>();
        bgRect.anchoredPosition = new Vector2(80, 0);
        bgRect.sizeDelta = new Vector2(200, 20);

        Image bgImg = barBg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.1f, 0.1f, 0.9f);

        GameObject barFill = new GameObject("HealthBarFill");
        barFill.transform.SetParent(barBg.transform);
        RectTransform fillRect = barFill.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
        fillRect.pivot = new Vector2(0, 0.5f);

        Image fillImg = barFill.AddComponent<Image>();
        fillImg.color = new Color(0.7f, 0.2f, 0.2f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;

        return fillImg;
    }

    void CreateBottomBar(Transform parent)
    {
        GameObject bottomBar = CreatePanel(parent, "BottomBar",
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0),
            new Vector2(0, 10), new Vector2(-40, 60));

        AddGothicBorder(bottomBar.transform, new Color(0.6f, 0.4f, 0.2f, 0.8f));

        Image bg = bottomBar.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.03f, 0.05f, 0.9f);

        GameObject progressContainer = new GameObject("WaveProgress");
        progressContainer.transform.SetParent(bottomBar.transform);
        RectTransform progRect = progressContainer.AddComponent<RectTransform>();
        progRect.anchorMin = new Vector2(0.3f, 0.2f);
        progRect.anchorMax = new Vector2(0.7f, 0.8f);
        progRect.offsetMin = Vector2.zero;
        progRect.offsetMax = Vector2.zero;

        Image progBg = progressContainer.AddComponent<Image>();
        progBg.color = new Color(0.1f, 0.08f, 0.08f, 0.9f);

        GameObject fillObj = new GameObject("ProgressFill");
        fillObj.transform.SetParent(progressContainer.transform);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
        fillRect.sizeDelta = new Vector2(0, 0);

        waveProgressFill = fillObj.AddComponent<Image>();
        waveProgressFill.color = new Color(0.8f, 0.6f, 0.2f);
    }

    void CreateKillCounter(Transform parent)
    {
        GameObject killContainer = CreatePanel(parent, "KillCounter",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -100), new Vector2(180, 80));

        Image bg = killContainer.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.02f, 0.02f, 0.85f);

        AddGothicBorder(killContainer.transform, new Color(0.5f, 0.2f, 0.2f, 0.7f));

        CreateGothicText(killContainer.transform, "KillLabel", "KILLS", 14,
            new Color(0.7f, 0.5f, 0.5f), FontStyles.Normal, 180).alignment = TextAlignmentOptions.Center;

        killCountText = CreateGothicText(killContainer.transform, "KillCount", "0", 42,
            new Color(0.9f, 0.3f, 0.3f), FontStyles.Bold, 180);
        killCountText.alignment = TextAlignmentOptions.Center;
        killCountText.rectTransform.anchoredPosition = new Vector2(0, -10);
    }

    void CreateComboMeter(Transform parent)
    {
        GameObject comboContainer = CreatePanel(parent, "ComboMeter",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 90), new Vector2(300, 50));

        Image bg = comboContainer.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0);

        comboText = CreateGothicText(comboContainer.transform, "ComboText", "", 28,
            new Color(1f, 0.8f, 0.3f), FontStyles.Bold, 300);
        comboText.alignment = TextAlignmentOptions.Center;

        GameObject fillBg = new GameObject("ComboFillBg");
        fillBg.transform.SetParent(comboContainer.transform);
        RectTransform fillBgRect = fillBg.AddComponent<RectTransform>();
        fillBgRect.anchorMin = new Vector2(0.1f, 0);
        fillBgRect.anchorMax = new Vector2(0.9f, 0.15f);
        fillBgRect.offsetMin = Vector2.zero;
        fillBgRect.offsetMax = Vector2.zero;

        Image fillBgImg = fillBg.AddComponent<Image>();
        fillBgImg.color = new Color(0.2f, 0.15f, 0.1f, 0.8f);

        GameObject fillObj = new GameObject("ComboFill");
        fillObj.transform.SetParent(fillBg.transform);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillRect.pivot = new Vector2(0, 0.5f);

        comboFill = fillObj.AddComponent<Image>();
        comboFill.color = new Color(1f, 0.6f, 0.2f, 0.9f);
        comboFill.type = Image.Type.Filled;
        comboFill.fillMethod = Image.FillMethod.Horizontal;
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        panel.AddComponent<Image>();

        return panel;
    }

    void AddGothicBorder(Transform parent, Color borderColor)
    {
        string[] sides = { "Top", "Bottom", "Left", "Right" };
        Vector2[] anchorsMin = { new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 0) };
        Vector2[] anchorsMax = { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        Vector2[] sizes = { new Vector2(0, 3), new Vector2(0, 3), new Vector2(3, 0), new Vector2(3, 0) };

        for (int i = 0; i < 4; i++)
        {
            GameObject border = new GameObject("Border_" + sides[i]);
            border.transform.SetParent(parent);

            RectTransform rect = border.AddComponent<RectTransform>();
            rect.anchorMin = anchorsMin[i];
            rect.anchorMax = anchorsMax[i];
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = sizes[i];

            Image img = border.AddComponent<Image>();
            img.color = borderColor;
        }
    }

    TextMeshProUGUI CreateGothicText(Transform parent, string name, string text, float size, Color color, FontStyles style, float width)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 50);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = color;

        return tmp;
    }

    void Update()
    {
        if (thrall == null && combatManager != null)
        {
            thrall = combatManager.GetThrall();
        }

        UpdateThrallHealth();
        UpdateCombo();
        UpdateBoostUI();
    }

    void UpdateBoostUI()
    {
        if (adManager == null) return;

        if (adManager.IsDamageBoostActive)
        {
            float remaining = adManager.DamageBoostTimeRemaining;
            if (boostTimerText != null)
            {
                boostTimerText.text = $"{Mathf.CeilToInt(remaining)}s";
            }
        }

        if (damageBoostGlow != null)
        {
            bool canOffer = adManager.CanOfferAd(AdType.DamageBoost) && !adManager.IsDamageBoostActive;
            damageBoostGlow.gameObject.SetActive(canOffer);

            if (canOffer)
            {
                float pulse = 0.3f + Mathf.Sin(Time.time * 4f) * 0.15f;
                Color c = damageBoostGlow.color;
                c.a = pulse;
                damageBoostGlow.color = c;
            }
        }
    }

    void UpdateThrallHealth()
    {
        if (thrall == null || thrallHealthFill == null) return;

        float ratio = thrall.CurrentHealth / thrall.Stats.maxHealth;
        thrallHealthFill.fillAmount = ratio;

        if (ratio < 0.3f)
        {
            thrallHealthFill.color = new Color(0.9f, 0.2f, 0.2f);
        }
        else if (ratio < 0.6f)
        {
            thrallHealthFill.color = new Color(0.9f, 0.6f, 0.2f);
        }
        else
        {
            thrallHealthFill.color = new Color(0.3f, 0.8f, 0.3f);
        }
    }

    void UpdateCombo()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboFill != null)
            {
                comboFill.fillAmount = comboTimer / maxComboTime;
            }

            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    void UpdateCurrency(int dusken, int shards)
    {
        if (duskenText != null) duskenText.text = FormatNumber(dusken);
        if (shardsText != null) shardsText.text = FormatNumber(shards);
    }

    void OnWaveStart(int wave)
    {
        currentWave = wave;
        enemiesKilledInWave = 0;
        enemiesInWave = hordeSpawner != null ? hordeSpawner.GetTotalEnemiesInWave() : 5 + wave * 2;

        if (waveText != null)
        {
            waveText.text = "WAVE " + ToRomanNumeral(wave);
        }

        UpdateWaveProgress();
    }

    void UpdateWaveProgress()
    {
        if (waveProgressFill == null || enemiesInWave == 0) return;

        float progress = (float)enemiesKilledInWave / enemiesInWave;
        waveProgressFill.rectTransform.anchorMax = new Vector2(progress, 1);
    }

    public void OnEnemyKilled()
    {
        killCount++;
        enemiesKilledInWave++;

        if (killCountText != null)
        {
            killCountText.text = killCount.ToString();

            killCountText.transform.localScale = Vector3.one * 1.3f;
        }

        comboCount++;
        comboTimer = maxComboTime;

        UpdateComboText();
        UpdateWaveProgress();

        OnComboChanged?.Invoke(comboCount);

        CameraEffects.Instance?.ShakeOnKill();
        ScreenEffects.Instance?.FlashOnKill();
    }

    void UpdateComboText()
    {
        if (comboText == null) return;

        if (comboCount >= 3)
        {
            string comboName = GetComboName(comboCount);
            comboText.text = $"{comboCount}x {comboName}!";
            comboText.transform.localScale = Vector3.one * (1f + comboCount * 0.05f);
        }
        else
        {
            comboText.text = "";
        }
    }

    string GetComboName(int count)
    {
        if (count >= 20) return "MASSACRE";
        if (count >= 15) return "SLAUGHTER";
        if (count >= 10) return "CARNAGE";
        if (count >= 7) return "RAMPAGE";
        if (count >= 5) return "BLOODBATH";
        if (count >= 3) return "COMBO";
        return "";
    }

    void ResetCombo()
    {
        comboCount = 0;
        if (comboText != null) comboText.text = "";
        if (comboFill != null) comboFill.fillAmount = 0;
    }

    string FormatNumber(int num)
    {
        if (num >= 1000000) return (num / 1000000f).ToString("0.#") + "M";
        if (num >= 1000) return (num / 1000f).ToString("0.#") + "K";
        return num.ToString();
    }

    string ToRomanNumeral(int num)
    {
        if (num > 20) return num.ToString();

        string[] numerals = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X",
                              "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX" };
        return numerals[Mathf.Clamp(num, 0, 20)];
    }

    void LateUpdate()
    {
        if (killCountText != null && killCountText.transform.localScale.x > 1f)
        {
            killCountText.transform.localScale = Vector3.Lerp(
                killCountText.transform.localScale, Vector3.one, Time.deltaTime * 8f);
        }

        if (comboText != null && comboText.transform.localScale.x > 1f)
        {
            comboText.transform.localScale = Vector3.Lerp(
                comboText.transform.localScale, Vector3.one, Time.deltaTime * 5f);
        }
    }

    void OnDestroy()
    {
        if (currencyManager != null)
            currencyManager.OnCurrencyChanged -= UpdateCurrency;
        if (hordeSpawner != null)
            hordeSpawner.OnWaveStarted -= OnWaveStart;
        if (adManager != null)
        {
            adManager.OnDamageBoostStarted -= OnDamageBoostStarted;
            adManager.OnDamageBoostEnded -= OnDamageBoostEnded;
        }
        if (questManager != null)
        {
            questManager.OnQuestCompleted -= OnQuestCompleted;
            questManager.OnQuestClaimed -= OnQuestStatusChanged;
        }
    }
}

