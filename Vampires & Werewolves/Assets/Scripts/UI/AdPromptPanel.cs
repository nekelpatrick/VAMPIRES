using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdPromptPanel : MonoBehaviour, IThemeable
{
    public static AdPromptPanel Instance { get; private set; }

    private UITheme Theme => UIThemeManager.Theme;

    [Header("Panel Elements")]
    private GameObject canvasRoot;
    private GameObject panelRoot;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI rewardText;
    private Button watchButton;
    private Button skipButton;
    private Image iconImage;
    private Image glowEffect;

    private Action onWatchClicked;
    private Action onSkipClicked;
    private AdType currentAdType;
    private bool isShowing;

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

        UIThemeManager.Instance?.RegisterElement(this);
    }

    public void ApplyTheme(UITheme theme)
    {
    }

    void Start()
    {
        CreatePanel();
        HidePanel();

        AdRewardManager adManager = AdRewardManager.Instance;
        if (adManager != null)
        {
            adManager.OnAdOffered += OnAdOffered;
        }
    }

    void CreatePanel()
    {
        bool isMobile = MobileUIScaler.Instance != null && MobileUIScaler.Instance.IsMobile;
        float scaleFactor = MobileUIScaler.Instance != null ? MobileUIScaler.Instance.ScaleFactor : 1f;

        canvasRoot = new GameObject("AdPromptCanvas");
        canvasRoot.transform.SetParent(transform);
        GameObject canvasObj = canvasRoot;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = isMobile ? 0.5f : 1f;

        canvasObj.AddComponent<GraphicRaycaster>();

        if (MobileUIScaler.Instance != null)
        {
            MobileUIScaler.Instance.ApplyToCanvas(canvas);
        }

        GameObject dimmer = CreateImage(canvasObj.transform, "Dimmer", new Color(0, 0, 0, 0.85f));
        RectTransform dimRect = dimmer.GetComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;

        float panelWidth = isMobile ? 800 : 700;
        float panelHeight = isMobile ? 550 : 500;

        panelRoot = new GameObject("PanelRoot");
        panelRoot.transform.SetParent(canvasObj.transform);
        RectTransform panelRect = panelRoot.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRect.anchoredPosition = Vector2.zero;

        GameObject panelBg = CreateImage(panelRoot.transform, "Background", new Color(0.08f, 0.05f, 0.1f, 0.98f));
        RectTransform bgRect = panelBg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject border = CreateImage(panelRoot.transform, "Border", new Color(0.6f, 0.4f, 0.2f, 0.9f));
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-4, -4);
        borderRect.offsetMax = new Vector2(4, 4);
        border.transform.SetAsFirstSibling();

        GameObject glowObj = CreateImage(panelRoot.transform, "Glow", new Color(1f, 0.8f, 0.3f, 0.15f));
        glowEffect = glowObj.GetComponent<Image>();
        RectTransform glowRect = glowObj.GetComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.sizeDelta = new Vector2(800, 600);
        glowObj.transform.SetAsFirstSibling();

        titleText = CreateText(panelRoot.transform, "Title", "DOUBLE YOUR LOOT!", 48,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(600, 70));
        titleText.color = new Color(1f, 0.85f, 0.3f);
        titleText.fontStyle = FontStyles.Bold;

        descriptionText = CreateText(panelRoot.transform, "Description", "Watch a short video to claim your reward!", 24,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(550, 60));
        descriptionText.color = new Color(0.8f, 0.8f, 0.8f);

        rewardText = CreateText(panelRoot.transform, "Reward", "+500 DUSKEN COIN", 36,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(500, 60));
        rewardText.color = new Color(0.3f, 1f, 0.4f);
        rewardText.fontStyle = FontStyles.Bold;

        float watchBtnWidth = isMobile ? 380 : 300;
        float watchBtnHeight = isMobile ? 90 : 70;
        float skipBtnWidth = isMobile ? 260 : 200;
        float skipBtnHeight = isMobile ? 65 : 50;

        watchButton = CreateButton(panelRoot.transform, "WatchButton", "WATCH AD",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(watchBtnWidth, watchBtnHeight),
            new Color(0.2f, 0.7f, 0.3f));
        watchButton.onClick.AddListener(OnWatchButtonClicked);

        skipButton = CreateButton(panelRoot.transform, "SkipButton", "NO THANKS",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(skipBtnWidth, skipBtnHeight),
            new Color(0.3f, 0.25f, 0.3f));
        skipButton.onClick.AddListener(OnSkipButtonClicked);
    }

    GameObject CreateImage(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);

        RectTransform rect = obj.AddComponent<RectTransform>();
        Image img = obj.AddComponent<Image>();
        img.color = color;

        return obj;
    }

    TextMeshProUGUI CreateText(Transform parent, string name, string content, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;

        return text;
    }

    Button CreateButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color bgColor)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = obj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = bgColor * 1.2f;
        colors.pressedColor = bgColor * 0.8f;
        btn.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;

        return btn;
    }

    void OnAdOffered(AdType type)
    {
        ShowPrompt(type);
    }

    public void ShowPrompt(AdType type)
    {
        currentAdType = type;
        isShowing = true;

        UpdateContent(type);

        if (canvasRoot != null)
            canvasRoot.SetActive(true);
        panelRoot.SetActive(true);
        Time.timeScale = 0f;
    }

    void UpdateContent(AdType type)
    {
        switch (type)
        {
            case AdType.DoubleLoot:
                titleText.text = "DOUBLE YOUR LOOT!";
                descriptionText.text = "Watch a short video to double your wave rewards!";
                rewardText.text = "2x DUSKEN COIN";
                titleText.color = new Color(1f, 0.85f, 0.3f);
                break;

            case AdType.InstantRevive:
                titleText.text = "REVIVE YOUR THRALL!";
                descriptionText.text = "Your werewolf has fallen. Watch a video to revive instantly!";
                rewardText.text = "INSTANT REVIVE";
                titleText.color = new Color(0.3f, 1f, 0.4f);
                break;

            case AdType.DamageBoost:
                titleText.text = "UNLEASH THE BEAST!";
                descriptionText.text = "Watch a video to boost your damage for 60 seconds!";
                rewardText.text = "2x DAMAGE (60s)";
                titleText.color = new Color(1f, 0.3f, 0.3f);
                break;

            case AdType.DoubleOffline:
                titleText.text = "WELCOME BACK!";
                int earnings = AdRewardManager.Instance?.PendingOfflineEarnings ?? 0;
                descriptionText.text = $"You earned {earnings} Dusken Coin while away!";
                rewardText.text = $"+{earnings} BONUS";
                titleText.color = new Color(0.5f, 0.7f, 1f);
                break;
        }
    }

    void OnWatchButtonClicked()
    {
        HidePanel();

        AdRewardManager adManager = AdRewardManager.Instance;
        if (adManager == null) return;

        switch (currentAdType)
        {
            case AdType.DoubleLoot:
                adManager.RequestDoubleLoot(OnAdResult);
                break;
            case AdType.InstantRevive:
                adManager.RequestInstantRevive(OnAdResult);
                break;
            case AdType.DamageBoost:
                adManager.RequestDamageBoost(OnAdResult);
                break;
            case AdType.DoubleOffline:
                adManager.RequestDoubleOffline(OnAdResult);
                break;
        }
    }

    void OnAdResult(bool success)
    {
        if (success)
        {
            ScreenEffects.Instance?.FlashGold(0.3f);
        }
    }

    void OnSkipButtonClicked()
    {
        HidePanel();
        onSkipClicked?.Invoke();
    }

    void HidePanel()
    {
        isShowing = false;
        if (canvasRoot != null)
            canvasRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (glowEffect != null && isShowing)
        {
            float pulse = 0.1f + Mathf.Sin(Time.unscaledTime * 3f) * 0.05f;
            Color c = glowEffect.color;
            c.a = pulse;
            glowEffect.color = c;
        }
    }

    void OnDestroy()
    {
        UIThemeManager.Instance?.UnregisterElement(this);

        AdRewardManager adManager = AdRewardManager.Instance;
        if (adManager != null)
        {
            adManager.OnAdOffered -= OnAdOffered;
        }

        Time.timeScale = 1f;
    }
}

