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
    private Canvas panelCanvas;
    private GameObject canvasRoot;
    private GameObject panelRoot;
    private Image panelBackground;
    private CanvasGroup canvasGroup;

    [Header("Header")]
    private Image headerBackground;
    private TextMeshProUGUI titleText;

    [Header("Tabs")]
    private Button upgradesTabButton;
    private Image upgradesTabImage;
    private TextMeshProUGUI upgradesTabText;
    private Button itemsTabButton;
    private Image itemsTabImage;
    private TextMeshProUGUI itemsTabText;
    private Button subscriptionTabButton;
    private Image subscriptionTabImage;
    private TextMeshProUGUI subscriptionTabText;
    private ShopTab currentTab = ShopTab.Upgrades;

    [Header("Content")]
    private Image contentBackground;
    private Transform upgradesContent;
    private Transform itemsContent;
    private Transform subscriptionContent;

    [Header("Close")]
    private Button closeButton;
    private Image closeButtonImage;
    private TextMeshProUGUI closeButtonText;

    private List<Image> itemCardBackgrounds = new List<Image>();
    private List<TextMeshProUGUI> itemNameTexts = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> itemDescTexts = new List<TextMeshProUGUI>();
    private List<Image> buyButtonImages = new List<Image>();
    private List<TextMeshProUGUI> priceTexts = new List<TextMeshProUGUI>();

    private CurrencyManager currencyManager;
    private bool isOpen;
    private bool isInitialized;
    private float inputBlockTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ApplyTheme(UITheme theme)
    {
        if (theme == null) return;

        if (panelBackground != null)
            panelBackground.color = theme.backgroundPanel;

        if (headerBackground != null)
            headerBackground.color = theme.backgroundDark;

        if (titleText != null)
            titleText.color = theme.textGold;

        if (contentBackground != null)
            contentBackground.color = theme.backgroundDark;

        if (closeButtonImage != null)
            closeButtonImage.color = theme.dangerColor;

        if (closeButtonText != null)
            closeButtonText.color = theme.textPrimary;

        foreach (var img in itemCardBackgrounds)
        {
            if (img != null) img.color = theme.backgroundLight;
        }

        foreach (var txt in itemNameTexts)
        {
            if (txt != null) txt.color = theme.textGold;
        }

        foreach (var txt in itemDescTexts)
        {
            if (txt != null) txt.color = theme.textSecondary;
        }

        foreach (var img in buyButtonImages)
        {
            if (img != null) img.color = theme.accentBrown;
        }

        UpdateTabButtonColors();
    }

    void Start()
    {
        Initialize();
        UIThemeManager.Instance?.RegisterElement(this);
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
        canvasRoot = UIFactory.CreateCanvas("ShopPanel_Canvas", transform, 200);
        panelCanvas = canvasRoot.GetComponent<Canvas>();

        panelRoot = UIFactory.CreatePanel("PanelRoot", canvasRoot.transform);
        RectTransform rootRect = panelRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image dimmer = panelRoot.GetComponent<Image>();
        dimmer.color = new Color(0, 0, 0, 0.85f);

        canvasGroup = panelRoot.AddComponent<CanvasGroup>();

        CreateMainPanel();
    }

    void CreateMainPanel()
    {
        GameObject mainPanel = UIFactory.CreatePanel("MainPanel", panelRoot.transform);
        RectTransform panelRect = mainPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        panelBackground = mainPanel.GetComponent<Image>();
        panelBackground.color = Theme.backgroundPanel;

        UIFactory.AddBorder(mainPanel.transform, Theme.borderGold, 3);

        CreateHeader(mainPanel.transform);
        CreateTabs(mainPanel.transform);
        CreateContentAreas(mainPanel.transform);
        CreateCloseButton(mainPanel.transform);
    }

    void CreateHeader(Transform parent)
    {
        GameObject header = UIFactory.CreatePanel("Header", parent);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = new Vector2(0, -10);
        headerRect.sizeDelta = new Vector2(-40, 80);

        headerBackground = header.GetComponent<Image>();
        headerBackground.color = Theme.backgroundDark;

        titleText = UIFactory.CreateTitle("Title", header.transform, "BLOOD MARKET").GetComponent<TextMeshProUGUI>();
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = new Vector2(20, 10);
        titleRect.offsetMax = new Vector2(-20, -10);
        titleText.fontSize = Theme.GetScaledFontSize(48);
        titleText.alignment = TextAlignmentOptions.Center;
    }

    void CreateTabs(Transform parent)
    {
        GameObject tabBar = UIFactory.CreatePanel("TabBar", parent);
        RectTransform tabRect = tabBar.GetComponent<RectTransform>();
        tabRect.anchorMin = new Vector2(0, 1);
        tabRect.anchorMax = new Vector2(1, 1);
        tabRect.pivot = new Vector2(0.5f, 1);
        tabRect.anchoredPosition = new Vector2(0, -100);
        tabRect.sizeDelta = new Vector2(-40, 60);

        tabBar.GetComponent<Image>().color = Color.clear;

        HorizontalLayoutGroup layout = tabBar.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = Theme.spacingMedium;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;

        var upgradesResult = CreateTabButton(tabBar.transform, "UPGRADES", () => { if (CanProcessInput()) SwitchTab(ShopTab.Upgrades); });
        upgradesTabButton = upgradesResult.Item1;
        upgradesTabImage = upgradesResult.Item2;
        upgradesTabText = upgradesResult.Item3;

        var itemsResult = CreateTabButton(tabBar.transform, "ITEMS", () => { if (CanProcessInput()) SwitchTab(ShopTab.Items); });
        itemsTabButton = itemsResult.Item1;
        itemsTabImage = itemsResult.Item2;
        itemsTabText = itemsResult.Item3;

        var subscriptionResult = CreateTabButton(tabBar.transform, "[ASHEN ONE]", () => { if (CanProcessInput()) SwitchTab(ShopTab.Subscription); });
        subscriptionTabButton = subscriptionResult.Item1;
        subscriptionTabImage = subscriptionResult.Item2;
        subscriptionTabText = subscriptionResult.Item3;
    }

    (Button, Image, TextMeshProUGUI) CreateTabButton(Transform parent, string label, Action onClick)
    {
        GameObject btnObj = UIFactory.CreateButton(label + "Tab", parent, label, onClick);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(200, 50);

        Button btn = btnObj.GetComponent<Button>();
        Image btnBg = btnObj.GetComponent<Image>();
        btnBg.color = Theme.backgroundLight;

        TextMeshProUGUI text = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        text.fontSize = Theme.GetScaledFontSize(24);
        text.color = Theme.textSecondary;

        return (btn, btnBg, text);
    }

    void CreateContentAreas(Transform parent)
    {
        GameObject contentArea = UIFactory.CreatePanel("ContentArea", parent);
        RectTransform contentRect = contentArea.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = new Vector2(20, 80);
        contentRect.offsetMax = new Vector2(-20, -170);

        contentBackground = contentArea.GetComponent<Image>();
        contentBackground.color = Theme.backgroundDark;

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
        GameObject container = UIFactory.CreatePanel(name, parent);
        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10, 10);
        rect.offsetMax = new Vector2(-10, -10);

        container.GetComponent<Image>().color = Color.clear;

        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = Theme.spacingMedium;
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
        CreateItemCard(parent, statName, description, duskenCost, shardCost, () =>
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
        CreateItemCard(parent, itemName, description, duskenCost, shardCost, () =>
        {
            Debug.Log($"[ShopPanel] Purchase {itemId} clicked");
            OnItemPurchased?.Invoke(itemId);
        });
    }

    void PopulateSubscriptionTab()
    {
        GameObject subscriptionCard = UIFactory.CreatePanel("SubscriptionCard", subscriptionContent);
        RectTransform cardRect = subscriptionCard.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(-20, 400);

        LayoutElement layoutElement = subscriptionCard.AddComponent<LayoutElement>();
        layoutElement.minHeight = 400;
        layoutElement.preferredHeight = 400;

        Image cardBg = subscriptionCard.GetComponent<Image>();
        cardBg.color = Theme.backgroundLight;
        itemCardBackgrounds.Add(cardBg);

        UIFactory.AddBorder(subscriptionCard.transform, Theme.textGold, 3);

        TextMeshProUGUI subTitleText = UIFactory.CreateTitle("Title", subscriptionCard.transform, "[ASHEN ONE]").GetComponent<TextMeshProUGUI>();
        RectTransform subTitleRect = subTitleText.GetComponent<RectTransform>();
        subTitleRect.anchorMin = new Vector2(0, 1);
        subTitleRect.anchorMax = new Vector2(1, 1);
        subTitleRect.pivot = new Vector2(0.5f, 1);
        subTitleRect.anchoredPosition = new Vector2(0, -20);
        subTitleRect.sizeDelta = new Vector2(-40, 60);
        subTitleText.fontSize = Theme.GetScaledFontSize(42);
        subTitleText.alignment = TextAlignmentOptions.Center;
        itemNameTexts.Add(subTitleText);

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
            TextMeshProUGUI benefitText = UIFactory.CreateText("Benefit", subscriptionCard.transform, benefit, Theme.fontSizeSmall).GetComponent<TextMeshProUGUI>();
            RectTransform benefitRect = benefitText.GetComponent<RectTransform>();
            benefitRect.anchorMin = new Vector2(0, 1);
            benefitRect.anchorMax = new Vector2(1, 1);
            benefitRect.pivot = new Vector2(0.5f, 1);
            benefitRect.anchoredPosition = new Vector2(0, yOffset);
            benefitRect.sizeDelta = new Vector2(-60, 30);
            benefitText.alignment = TextAlignmentOptions.MidlineLeft;
            benefitText.color = Theme.textSecondary;

            yOffset -= 35;
        }

        GameObject subscribeBtn = UIFactory.CreateButton("SubscribeButton", subscriptionCard.transform, "SUBSCRIBE", () =>
        {
            Debug.Log("[ShopPanel] Subscribe clicked");
            OnItemPurchased?.Invoke("subscription_ashen_one");
        });

        RectTransform btnRect = subscribeBtn.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0);
        btnRect.anchorMax = new Vector2(0.5f, 0);
        btnRect.pivot = new Vector2(0.5f, 0);
        btnRect.anchoredPosition = new Vector2(0, 30);
        btnRect.sizeDelta = new Vector2(300, 60);

        Image btnBg = subscribeBtn.GetComponent<Image>();
        btnBg.color = Theme.accentBrown;
        buyButtonImages.Add(btnBg);

        TextMeshProUGUI btnText = subscribeBtn.GetComponentInChildren<TextMeshProUGUI>();
        btnText.fontSize = Theme.GetScaledFontSize(28);
        btnText.color = Theme.textPrimary;
    }

    void CreateItemCard(Transform parent, string itemName, string description, int duskenCost, int shardCost, Action onPurchase)
    {
        GameObject card = UIFactory.CreatePanel(itemName + "_Card", parent);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(-20, 100);

        LayoutElement layoutElement = card.AddComponent<LayoutElement>();
        layoutElement.minHeight = 100;
        layoutElement.preferredHeight = 100;

        Image cardBg = card.GetComponent<Image>();
        cardBg.color = Theme.backgroundLight;
        itemCardBackgrounds.Add(cardBg);

        UIFactory.AddBorder(card.transform, Theme.borderGold, 2);

        TextMeshProUGUI nameText = UIFactory.CreateText("Name", card.transform, itemName, Theme.fontSizeMedium).GetComponent<TextMeshProUGUI>();
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(0.6f, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.anchoredPosition = new Vector2(15, -15);
        nameRect.sizeDelta = new Vector2(0, 35);
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.MidlineLeft;
        nameText.color = Theme.textGold;
        itemNameTexts.Add(nameText);

        TextMeshProUGUI descText = UIFactory.CreateText("Description", card.transform, description, Theme.fontSizeSmall).GetComponent<TextMeshProUGUI>();
        RectTransform descRect = descText.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0);
        descRect.anchorMax = new Vector2(0.6f, 1);
        descRect.pivot = new Vector2(0, 1);
        descRect.anchoredPosition = new Vector2(15, -50);
        descRect.sizeDelta = new Vector2(0, 50);
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.color = Theme.textSecondary;
        itemDescTexts.Add(descText);

        TextMeshProUGUI priceText = UIFactory.CreateText("Price", card.transform, "", Theme.fontSizeMedium).GetComponent<TextMeshProUGUI>();
        RectTransform priceRect = priceText.GetComponent<RectTransform>();
        priceRect.anchorMin = new Vector2(0.6f, 0.5f);
        priceRect.anchorMax = new Vector2(0.85f, 0.5f);
        priceRect.pivot = new Vector2(0.5f, 0.5f);
        priceRect.anchoredPosition = Vector2.zero;
        priceRect.sizeDelta = new Vector2(0, 40);
        priceText.fontStyle = FontStyles.Bold;
        priceText.alignment = TextAlignmentOptions.Center;

        if (duskenCost > 0)
        {
            priceText.text = $"{duskenCost} DC";
            priceText.color = Theme.textGold;
        }
        else if (shardCost > 0)
        {
            priceText.text = $"{shardCost} BS";
            priceText.color = Theme.bloodRed;
        }
        priceTexts.Add(priceText);

        GameObject buyBtn = UIFactory.CreateButton("BuyButton", card.transform, "BUY", onPurchase);
        RectTransform btnRect = buyBtn.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.85f, 0.2f);
        btnRect.anchorMax = new Vector2(0.98f, 0.8f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        Image btnBg = buyBtn.GetComponent<Image>();
        btnBg.color = Theme.accentBrown;
        buyButtonImages.Add(btnBg);

        TextMeshProUGUI btnText = buyBtn.GetComponentInChildren<TextMeshProUGUI>();
        btnText.fontSize = Theme.GetScaledFontSize(20);
        btnText.color = Theme.textPrimary;
    }

    void CreateCloseButton(Transform parent)
    {
        closeButton = UIFactory.CreateIconButton("CloseButton", parent, "X", Hide);
        RectTransform btnRect = closeButton.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-10, -10);
        btnRect.sizeDelta = new Vector2(Theme.GetScaledSize(50), Theme.GetScaledSize(50));

        closeButtonImage = closeButton.GetComponent<Image>();
        closeButtonImage.color = Theme.dangerColor;

        closeButtonText = closeButton.GetComponentInChildren<TextMeshProUGUI>();
        closeButtonText.fontSize = Theme.GetScaledFontSize(32);
        closeButtonText.color = Theme.textPrimary;
    }

    void SwitchTab(ShopTab tab)
    {
        currentTab = tab;

        if (upgradesContent != null)
            upgradesContent.gameObject.SetActive(tab == ShopTab.Upgrades);
        if (itemsContent != null)
            itemsContent.gameObject.SetActive(tab == ShopTab.Items);
        if (subscriptionContent != null)
            subscriptionContent.gameObject.SetActive(tab == ShopTab.Subscription);

        UpdateTabButtonColors();
    }

    void UpdateTabButtonColors()
    {
        Color activeColor = Theme != null ? Theme.backgroundPanel : new Color(0.25f, 0.18f, 0.15f, 1f);
        Color inactiveColor = Theme != null ? Theme.backgroundLight : new Color(0.15f, 0.1f, 0.12f, 0.9f);
        Color activeTextColor = Theme != null ? Theme.textGold : new Color(1f, 0.85f, 0.4f);
        Color inactiveTextColor = Theme != null ? Theme.textSecondary : new Color(0.7f, 0.65f, 0.6f);

        if (upgradesTabImage != null)
            upgradesTabImage.color = currentTab == ShopTab.Upgrades ? activeColor : inactiveColor;
        if (upgradesTabText != null)
            upgradesTabText.color = currentTab == ShopTab.Upgrades ? activeTextColor : inactiveTextColor;

        if (itemsTabImage != null)
            itemsTabImage.color = currentTab == ShopTab.Items ? activeColor : inactiveColor;
        if (itemsTabText != null)
            itemsTabText.color = currentTab == ShopTab.Items ? activeTextColor : inactiveTextColor;

        if (subscriptionTabImage != null)
            subscriptionTabImage.color = currentTab == ShopTab.Subscription ? activeColor : inactiveColor;
        if (subscriptionTabText != null)
            subscriptionTabText.color = currentTab == ShopTab.Subscription ? activeTextColor : inactiveTextColor;
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

    void OnDestroy()
    {
        UIThemeManager.Instance?.UnregisterElement(this);
    }
}
