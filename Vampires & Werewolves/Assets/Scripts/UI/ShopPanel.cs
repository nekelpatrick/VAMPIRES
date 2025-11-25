using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ShopTab
{
    Upgrades,
    Items,
    Subscription
}

public class ShopPanel : MonoBehaviour, IThemeable
{
    public static ShopPanel Instance { get; private set; }

    private UITheme Theme => UIThemeManager.Theme;

    public event Action OnShopOpened;
    public event Action OnShopClosed;
    public event Action<string> OnItemPurchased;

    [Header("Main Panel")]
    private GameObject panelRoot;
    private CanvasGroup canvasGroup;

    [Header("Tabs")]
    private Button upgradesTabButton;
    private Button itemsTabButton;
    private Button subscriptionTabButton;
    private ShopTab currentTab = ShopTab.Upgrades;

    [Header("Content")]
    private Transform upgradesContent;
    private Transform itemsContent;
    private Transform subscriptionContent;

    [Header("Close")]
    private Button closeButton;

    private CurrencyManager currencyManager;
    private bool isOpen;
    private bool isInitialized;
    private float inputBlockTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        UIThemeManager.Instance?.RegisterElement(this);
    }

    public void ApplyTheme(UITheme theme)
    {
    }

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        if (inputBlockTime > 0)
        {
            inputBlockTime -= Time.deltaTime;
        }
    }

    bool CanProcessInput()
    {
        return inputBlockTime <= 0;
    }

    public void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

        currencyManager = CurrencyManager.Instance;
        CreateShopUI();
        Hide();
    }

    void CreateShopUI()
    {
        GameObject canvasObj = new GameObject("ShopPanel_Canvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        panelRoot = new GameObject("PanelRoot");
        panelRoot.transform.SetParent(canvasObj.transform);

        RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image bgOverlay = panelRoot.AddComponent<Image>();
        bgOverlay.color = new Color(0, 0, 0, 0.85f);

        canvasGroup = panelRoot.AddComponent<CanvasGroup>();

        CreateMainPanel();
    }

    void CreateMainPanel()
    {
        GameObject mainPanel = new GameObject("MainPanel");
        mainPanel.transform.SetParent(panelRoot.transform);

        RectTransform panelRect = mainPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = mainPanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.05f, 0.08f, 0.98f);

        AddGothicBorder(mainPanel.transform, new Color(0.7f, 0.5f, 0.2f, 0.9f));

        CreateHeader(mainPanel.transform);
        CreateTabs(mainPanel.transform);
        CreateContentAreas(mainPanel.transform);
        CreateCloseButton(mainPanel.transform);
    }

    void CreateHeader(Transform parent)
    {
        GameObject header = new GameObject("Header");
        header.transform.SetParent(parent);

        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = new Vector2(0, -10);
        headerRect.sizeDelta = new Vector2(-40, 80);

        Image headerBg = header.AddComponent<Image>();
        headerBg.color = new Color(0.12f, 0.08f, 0.1f, 0.95f);

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(header.transform);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = new Vector2(20, 10);
        titleRect.offsetMax = new Vector2(-20, -10);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "BLOOD MARKET";
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.9f, 0.7f, 0.3f);
    }

    void CreateTabs(Transform parent)
    {
        GameObject tabBar = new GameObject("TabBar");
        tabBar.transform.SetParent(parent);

        RectTransform tabRect = tabBar.AddComponent<RectTransform>();
        tabRect.anchorMin = new Vector2(0, 1);
        tabRect.anchorMax = new Vector2(1, 1);
        tabRect.pivot = new Vector2(0.5f, 1);
        tabRect.anchoredPosition = new Vector2(0, -100);
        tabRect.sizeDelta = new Vector2(-40, 60);

        HorizontalLayoutGroup layout = tabBar.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;

        upgradesTabButton = CreateTabButton(tabBar.transform, "UPGRADES", () => { if (CanProcessInput()) SwitchTab(ShopTab.Upgrades); });
        itemsTabButton = CreateTabButton(tabBar.transform, "ITEMS", () => { if (CanProcessInput()) SwitchTab(ShopTab.Items); });
        subscriptionTabButton = CreateTabButton(tabBar.transform, "[ASHEN ONE]", () => { if (CanProcessInput()) SwitchTab(ShopTab.Subscription); });
    }

    Button CreateTabButton(Transform parent, string label, Action onClick)
    {
        GameObject btnObj = new GameObject(label + "Tab");
        btnObj.transform.SetParent(parent);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(200, 50);

        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = new Color(0.15f, 0.1f, 0.12f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());

        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 24;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.85f, 0.75f, 0.6f);

        return btn;
    }

    void CreateContentAreas(Transform parent)
    {
        GameObject contentArea = new GameObject("ContentArea");
        contentArea.transform.SetParent(parent, false);

        RectTransform contentRect = contentArea.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = new Vector2(20, 80);
        contentRect.offsetMax = new Vector2(-20, -170);

        Image contentBg = contentArea.AddComponent<Image>();
        contentBg.color = new Color(0.08f, 0.05f, 0.07f, 0.9f);

        upgradesContent = CreateSimpleContent(contentArea.transform, "UpgradesContent");
        itemsContent = CreateSimpleContent(contentArea.transform, "ItemsContent");
        subscriptionContent = CreateSimpleContent(contentArea.transform, "SubscriptionContent");

        PopulateUpgradesTab();
        PopulateItemsTab();
        PopulateSubscriptionTab();

        SwitchTab(ShopTab.Upgrades);
    }

    Transform CreateSimpleContent(Transform parent, string name)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent, false);

        RectTransform rect = container.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10, 10);
        rect.offsetMax = new Vector2(-10, -10);

        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        return container.transform;
    }

    void PopulateUpgradesTab()
    {
        CreateUpgradeCard(upgradesContent, "ATTACK", "Increase your [THRALL]'s attack power", "attack", 100, 0);
        CreateUpgradeCard(upgradesContent, "DEFENSE", "Increase your [THRALL]'s defense", "defense", 80, 0);
        CreateUpgradeCard(upgradesContent, "MAX HP", "Increase your [THRALL]'s health pool", "maxHp", 120, 0);
        CreateUpgradeCard(upgradesContent, "SPEED", "Increase your [THRALL]'s attack speed", "speed", 150, 1);
    }

    void CreateUpgradeCard(Transform parent, string statName, string description, string statId, int duskenCost, int shardCost)
    {
        GameObject card = CreateItemCard(parent, statName, description, duskenCost, shardCost, () =>
        {
            Debug.Log($"[ShopPanel] Upgrade {statId} clicked");
            OnItemPurchased?.Invoke($"upgrade_{statId}");
        });
    }

    void PopulateItemsTab()
    {
        CreateShopItem(itemsContent, "Revive Token", "Instantly revive your [THRALL]", 500, 0, "revive_token_1");
        CreateShopItem(itemsContent, "XP Booster (30 min)", "2x XP from battles", 300, 0, "xp_booster_30m");
        CreateShopItem(itemsContent, "Small Coin Pack", "Get 1000 [DUSKEN COIN]", 0, 5, "dusken_pack_small");
        CreateShopItem(itemsContent, "Attack Boost", "Permanent +10 Attack", 2000, 0, "stat_boost_attack");
        CreateShopItem(itemsContent, "Defense Boost", "Permanent +8 Defense", 1500, 0, "stat_boost_defense");
        CreateShopItem(itemsContent, "HP Boost", "Permanent +50 Max HP", 1800, 0, "stat_boost_hp");
    }

    void CreateShopItem(Transform parent, string itemName, string description, int duskenCost, int shardCost, string itemId)
    {
        GameObject card = CreateItemCard(parent, itemName, description, duskenCost, shardCost, () =>
        {
            Debug.Log($"[ShopPanel] Purchase {itemId} clicked");
            OnItemPurchased?.Invoke(itemId);
        });
    }

    void PopulateSubscriptionTab()
    {
        GameObject subscriptionCard = new GameObject("SubscriptionCard");
        subscriptionCard.transform.SetParent(subscriptionContent, false);

        RectTransform cardRect = subscriptionCard.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(-20, 400);

        LayoutElement layoutElement = subscriptionCard.AddComponent<LayoutElement>();
        layoutElement.minHeight = 400;
        layoutElement.preferredHeight = 400;

        Image cardBg = subscriptionCard.AddComponent<Image>();
        cardBg.color = new Color(0.18f, 0.12f, 0.15f, 1f);

        AddGothicBorder(subscriptionCard.transform, new Color(0.9f, 0.7f, 0.2f, 0.8f));

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(subscriptionCard.transform);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(-40, 60);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "[ASHEN ONE]";
        titleText.fontSize = 42;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.85f, 0.4f);

        string[] benefits = {
            "• 50 [BLOOD SHARDS] monthly",
            "• 50% faster offline accumulation",
            "• 25% bonus loot from battles",
            "• 50% revival discount",
            "• Priority PvP queue",
            "• Exclusive cosmetics",
            "• Premium badge"
        };

        float yOffset = -100;
        foreach (string benefit in benefits)
        {
            GameObject benefitObj = new GameObject("Benefit");
            benefitObj.transform.SetParent(subscriptionCard.transform);

            RectTransform benefitRect = benefitObj.AddComponent<RectTransform>();
            benefitRect.anchorMin = new Vector2(0, 1);
            benefitRect.anchorMax = new Vector2(1, 1);
            benefitRect.pivot = new Vector2(0.5f, 1);
            benefitRect.anchoredPosition = new Vector2(0, yOffset);
            benefitRect.sizeDelta = new Vector2(-60, 30);

            TextMeshProUGUI benefitText = benefitObj.AddComponent<TextMeshProUGUI>();
            benefitText.text = benefit;
            benefitText.fontSize = 22;
            benefitText.alignment = TextAlignmentOptions.MidlineLeft;
            benefitText.color = new Color(0.8f, 0.75f, 0.7f);

            yOffset -= 35;
        }

        GameObject subscribeBtn = new GameObject("SubscribeButton");
        subscribeBtn.transform.SetParent(subscriptionCard.transform);

        RectTransform btnRect = subscribeBtn.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0);
        btnRect.anchorMax = new Vector2(0.5f, 0);
        btnRect.pivot = new Vector2(0.5f, 0);
        btnRect.anchoredPosition = new Vector2(0, 30);
        btnRect.sizeDelta = new Vector2(300, 60);

        Image btnBg = subscribeBtn.AddComponent<Image>();
        btnBg.color = new Color(0.7f, 0.5f, 0.15f, 1f);

        Button btn = subscribeBtn.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            Debug.Log("[ShopPanel] Subscribe clicked");
            OnItemPurchased?.Invoke("subscription_ashen_one");
        });

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(subscribeBtn.transform);

        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "SUBSCRIBE";
        btnText.fontSize = 28;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
    }

    GameObject CreateItemCard(Transform parent, string itemName, string description, int duskenCost, int shardCost, Action onPurchase)
    {
        GameObject card = new GameObject(itemName + "_Card");
        card.transform.SetParent(parent, false);

        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(-20, 100);

        LayoutElement layoutElement = card.AddComponent<LayoutElement>();
        layoutElement.minHeight = 100;
        layoutElement.preferredHeight = 100;

        Image cardBg = card.AddComponent<Image>();
        cardBg.color = new Color(0.15f, 0.1f, 0.12f, 1f);

        AddGothicBorder(card.transform, new Color(0.5f, 0.35f, 0.2f, 0.7f));

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(card.transform);

        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(0.6f, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.anchoredPosition = new Vector2(15, -15);
        nameRect.sizeDelta = new Vector2(0, 35);

        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = itemName;
        nameText.fontSize = 28;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.MidlineLeft;
        nameText.color = new Color(0.95f, 0.85f, 0.7f);

        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(card.transform);

        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0);
        descRect.anchorMax = new Vector2(0.6f, 1);
        descRect.pivot = new Vector2(0, 1);
        descRect.anchoredPosition = new Vector2(15, -50);
        descRect.sizeDelta = new Vector2(0, 50);

        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = description;
        descText.fontSize = 18;
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.color = new Color(0.7f, 0.65f, 0.6f);

        GameObject priceObj = new GameObject("Price");
        priceObj.transform.SetParent(card.transform);

        RectTransform priceRect = priceObj.AddComponent<RectTransform>();
        priceRect.anchorMin = new Vector2(0.6f, 0.5f);
        priceRect.anchorMax = new Vector2(0.85f, 0.5f);
        priceRect.pivot = new Vector2(0.5f, 0.5f);
        priceRect.anchoredPosition = Vector2.zero;
        priceRect.sizeDelta = new Vector2(0, 40);

        TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
        if (duskenCost > 0)
        {
            priceText.text = $"{duskenCost} DC";
            priceText.color = new Color(1f, 0.85f, 0.4f);
        }
        else if (shardCost > 0)
        {
            priceText.text = $"{shardCost} BS";
            priceText.color = new Color(0.9f, 0.3f, 0.35f);
        }
        priceText.fontSize = 24;
        priceText.fontStyle = FontStyles.Bold;
        priceText.alignment = TextAlignmentOptions.Center;

        GameObject buyBtn = new GameObject("BuyButton");
        buyBtn.transform.SetParent(card.transform);

        RectTransform btnRect = buyBtn.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.85f, 0.2f);
        btnRect.anchorMax = new Vector2(0.98f, 0.8f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        Image btnBg = buyBtn.AddComponent<Image>();
        btnBg.color = new Color(0.5f, 0.35f, 0.15f, 1f);

        Button btn = buyBtn.AddComponent<Button>();
        btn.onClick.AddListener(() => onPurchase?.Invoke());

        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.7f, 0.6f);
        btn.colors = colors;

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(buyBtn.transform);

        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "BUY";
        btnText.fontSize = 20;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;

        return card;
    }

    void CreateCloseButton(Transform parent)
    {
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(parent);

        RectTransform btnRect = closeBtn.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-10, -10);
        btnRect.sizeDelta = new Vector2(50, 50);

        Image btnBg = closeBtn.AddComponent<Image>();
        btnBg.color = new Color(0.6f, 0.2f, 0.2f, 0.9f);

        closeButton = closeBtn.AddComponent<Button>();
        closeButton.onClick.AddListener(Hide);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(closeBtn.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "X";
        text.fontSize = 32;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
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

    void SwitchTab(ShopTab tab)
    {
        currentTab = tab;

        if (upgradesContent != null)
        {
            upgradesContent.gameObject.SetActive(tab == ShopTab.Upgrades);
        }
        if (itemsContent != null)
        {
            itemsContent.gameObject.SetActive(tab == ShopTab.Items);
        }
        if (subscriptionContent != null)
        {
            subscriptionContent.gameObject.SetActive(tab == ShopTab.Subscription);
        }

        UpdateTabButtonColors();
    }

    void UpdateTabButtonColors()
    {
        Color activeColor = new Color(0.25f, 0.18f, 0.15f, 1f);
        Color inactiveColor = new Color(0.15f, 0.1f, 0.12f, 0.9f);

        if (upgradesTabButton != null)
            upgradesTabButton.GetComponent<Image>().color = currentTab == ShopTab.Upgrades ? activeColor : inactiveColor;
        if (itemsTabButton != null)
            itemsTabButton.GetComponent<Image>().color = currentTab == ShopTab.Items ? activeColor : inactiveColor;
        if (subscriptionTabButton != null)
            subscriptionTabButton.GetComponent<Image>().color = currentTab == ShopTab.Subscription ? activeColor : inactiveColor;
    }

    public void Show()
    {
        Initialize();

        if (panelRoot == null)
        {
            Debug.LogError("[ShopPanel] panelRoot is null after Initialize");
            return;
        }

        panelRoot.SetActive(true);
        isOpen = true;
        inputBlockTime = 0.2f;
        SwitchTab(ShopTab.Upgrades);

        OnShopOpened?.Invoke();
    }

    public void Hide()
    {
        if (panelRoot == null) return;

        panelRoot.SetActive(false);
        isOpen = false;
        OnShopClosed?.Invoke();
    }

    public void Toggle()
    {
        if (isOpen) Hide();
        else Show();
    }

    public bool IsOpen => isOpen;
}

