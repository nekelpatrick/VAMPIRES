using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdPromptPanel : MonoBehaviour, IThemeable
{
    public static AdPromptPanel Instance { get; private set; }

    private UITheme Theme => UIThemeManager.Theme;

    [Header("Panel Elements")]
    private Canvas panelCanvas;
    private GameObject canvasRoot;
    private GameObject panelRoot;
    private Image panelBackground;
    private Image dimmerImage;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI rewardText;
    private Button watchButton;
    private Image watchButtonImage;
    private Button skipButton;
    private Image skipButtonImage;
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
    }

    public void ApplyTheme(UITheme theme)
    {
        if (theme == null) return;

        if (dimmerImage != null)
            dimmerImage.color = theme.backgroundOverlay;

        if (panelBackground != null)
            panelBackground.color = theme.backgroundPanel;

        if (descriptionText != null)
            descriptionText.color = theme.textSecondary;

        if (skipButtonImage != null)
            skipButtonImage.color = theme.buttonNormal;
    }

    void Start()
    {
        CreatePanel();
        UIThemeManager.Instance?.RegisterElement(this);
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

        canvasRoot = new GameObject("AdPromptCanvas");
        canvasRoot.transform.SetParent(transform);

        panelCanvas = UIFactory.CreateCanvas(canvasRoot.transform, "Canvas", 500);

        GameObject dimmer = new GameObject("Dimmer");
        dimmer.transform.SetParent(canvasRoot.transform);
        RectTransform dimRect = dimmer.AddComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;
        dimmerImage = dimmer.AddComponent<Image>();
        dimmerImage.color = Theme.backgroundOverlay;

        float panelWidth = isMobile ? 800 : 700;
        float panelHeight = isMobile ? 550 : 500;

        panelRoot = UIFactory.CreatePanel(canvasRoot.transform, "PanelRoot", panelWidth, panelHeight);
        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;

        panelBackground = panelRoot.GetComponent<Image>();
        panelBackground.color = Theme.backgroundPanel;

        UIFactory.AddBorder(panelRoot.transform, Theme.borderGold);

        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(panelRoot.transform);
        glowObj.transform.SetAsFirstSibling();
        RectTransform glowRect = glowObj.AddComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.sizeDelta = new Vector2(panelWidth + 100, panelHeight + 100);
        glowEffect = glowObj.AddComponent<Image>();
        glowEffect.color = new Color(Theme.textGold.r, Theme.textGold.g, Theme.textGold.b, 0.15f);

        titleText = UIFactory.CreateText(panelRoot.transform, "Title", "DOUBLE YOUR LOOT!",
            Theme.GetScaledFontSize(Theme.fontSizeXXL), Theme.textGold, FontStyles.Bold);
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -Theme.spacingXL);
        titleRect.sizeDelta = new Vector2(600, 70);
        titleText.alignment = TextAlignmentOptions.Center;

        descriptionText = UIFactory.CreateText(panelRoot.transform, "Description", "Watch a short video to claim your reward!",
            Theme.GetScaledFontSize(Theme.fontSizeMD), Theme.textSecondary, FontStyles.Normal);
        RectTransform descRect = descriptionText.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.5f, 1);
        descRect.anchorMax = new Vector2(0.5f, 1);
        descRect.anchoredPosition = new Vector2(0, -120);
        descRect.sizeDelta = new Vector2(550, 60);
        descriptionText.alignment = TextAlignmentOptions.Center;

        rewardText = UIFactory.CreateText(panelRoot.transform, "Reward", "+500 DUSKEN COIN",
            Theme.GetScaledFontSize(Theme.fontSizeLG), Theme.textSuccess, FontStyles.Bold);
        RectTransform rewardRect = rewardText.GetComponent<RectTransform>();
        rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
        rewardRect.anchorMax = new Vector2(0.5f, 0.5f);
        rewardRect.anchoredPosition = new Vector2(0, Theme.spacingLG);
        rewardRect.sizeDelta = new Vector2(500, 60);
        rewardText.alignment = TextAlignmentOptions.Center;

        float watchBtnWidth = isMobile ? 380 : 300;
        float watchBtnHeight = isMobile ? 90 : 70;
        float skipBtnWidth = isMobile ? 260 : 200;
        float skipBtnHeight = isMobile ? 65 : 50;

        watchButton = UIFactory.CreateButton(panelRoot.transform, "WatchButton", "WATCH AD",
            watchBtnWidth, watchBtnHeight, null);
        watchButtonImage = watchButton.GetComponent<Image>();
        watchButtonImage.color = Theme.statusActive;
        RectTransform watchRect = watchButton.GetComponent<RectTransform>();
        watchRect.anchorMin = new Vector2(0.5f, 0);
        watchRect.anchorMax = new Vector2(0.5f, 0);
        watchRect.anchoredPosition = new Vector2(0, 150);
        watchButton.onClick.AddListener(OnWatchButtonClicked);

        skipButton = UIFactory.CreateButton(panelRoot.transform, "SkipButton", "NO THANKS",
            skipBtnWidth, skipBtnHeight, null);
        skipButtonImage = skipButton.GetComponent<Image>();
        skipButtonImage.color = Theme.buttonNormal;
        RectTransform skipRect = skipButton.GetComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(0.5f, 0);
        skipRect.anchorMax = new Vector2(0.5f, 0);
        skipRect.anchoredPosition = new Vector2(0, Theme.spacingXL);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
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
        if (panelRoot != null)
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
                titleText.color = Theme.textGold;
                rewardText.color = Theme.textGold;
                watchButtonImage.color = Theme.statusActive;
                break;

            case AdType.InstantRevive:
                titleText.text = "REVIVE YOUR THRALL!";
                descriptionText.text = "Your werewolf has fallen. Watch a video to revive instantly!";
                rewardText.text = "INSTANT REVIVE";
                titleText.color = Theme.textSuccess;
                rewardText.color = Theme.textSuccess;
                watchButtonImage.color = Theme.statusActive;
                break;

            case AdType.DamageBoost:
                titleText.text = "UNLEASH THE BEAST!";
                descriptionText.text = "Watch a video to boost your damage for 60 seconds!";
                rewardText.text = "2x DAMAGE (60s)";
                titleText.color = Theme.textBlood;
                rewardText.color = Theme.textBlood;
                watchButtonImage.color = Theme.borderBlood;
                break;

            case AdType.DoubleOffline:
                titleText.text = "WELCOME BACK!";
                int earnings = AdRewardManager.Instance?.PendingOfflineEarnings ?? 0;
                descriptionText.text = $"You earned {earnings} Dusken Coin while away!";
                rewardText.text = $"+{earnings} BONUS";
                titleText.color = Theme.fillProgress;
                rewardText.color = Theme.textGold;
                watchButtonImage.color = Theme.statusActive;
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
            float pulse = Theme.pulseIntensity + Mathf.Sin(Time.unscaledTime * Theme.pulseSpeed * 0.15f) * 0.05f;
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
