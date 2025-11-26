using System;
using System.Collections.Generic;
using UnityEngine;

public class UIThemeManager : MonoBehaviour
{
    public static UIThemeManager Instance { get; private set; }

    public static event Action<UITheme> OnThemeChanged;

    [SerializeField] private UITheme currentTheme;

    private static UITheme defaultTheme;
    private List<IThemeable> registeredElements = new List<IThemeable>();

    public static UITheme Theme
    {
        get
        {
            if (Instance != null && Instance.currentTheme != null)
            {
                return Instance.currentTheme;
            }

            if (defaultTheme == null)
            {
                defaultTheme = CreateDefaultTheme();
            }

            return defaultTheme;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (currentTheme == null)
            {
                currentTheme = CreateDefaultTheme();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetTheme(UITheme newTheme)
    {
        if (newTheme == null) return;

        currentTheme = newTheme;
        NotifyThemeChanged();
    }

    public void RegisterElement(IThemeable element)
    {
        if (element != null && !registeredElements.Contains(element))
        {
            registeredElements.Add(element);
            element.ApplyTheme(Theme);
        }
    }

    public void UnregisterElement(IThemeable element)
    {
        registeredElements.Remove(element);
    }

    void NotifyThemeChanged()
    {
        OnThemeChanged?.Invoke(currentTheme);

        for (int i = registeredElements.Count - 1; i >= 0; i--)
        {
            if (registeredElements[i] != null)
            {
                registeredElements[i].ApplyTheme(currentTheme);
            }
            else
            {
                registeredElements.RemoveAt(i);
            }
        }
    }

    static UITheme CreateDefaultTheme()
    {
        UITheme theme = ScriptableObject.CreateInstance<UITheme>();

        theme.backgroundPrimary = new Color(0.04f, 0.02f, 0.05f, 0.95f);
        theme.backgroundSecondary = new Color(0.08f, 0.04f, 0.06f, 0.95f);
        theme.backgroundPanel = new Color(0.05f, 0.03f, 0.05f, 0.95f);
        theme.backgroundDark = new Color(0.02f, 0.01f, 0.03f, 1f);
        theme.backgroundLight = new Color(0.15f, 0.1f, 0.12f, 0.9f);
        theme.backgroundOverlay = new Color(0f, 0f, 0f, 0.7f);

        theme.borderGold = new Color(0.7f, 0.5f, 0.2f, 0.8f);
        theme.borderBronze = new Color(0.6f, 0.4f, 0.2f, 0.8f);
        theme.borderBlood = new Color(0.6f, 0.15f, 0.15f, 0.8f);
        theme.borderSubtle = new Color(0.5f, 0.35f, 0.2f, 0.6f);

        theme.textPrimary = Color.white;
        theme.textSecondary = new Color(0.8f, 0.75f, 0.7f, 1f);
        theme.textMuted = new Color(0.6f, 0.55f, 0.5f, 1f);
        theme.textGold = new Color(1f, 0.85f, 0.3f, 1f);
        theme.textBlood = new Color(0.9f, 0.2f, 0.25f, 1f);
        theme.textAccent = new Color(0.9f, 0.8f, 0.6f, 1f);
        theme.textSuccess = new Color(0.4f, 0.8f, 0.3f, 1f);

        theme.fillHealth = new Color(0.8f, 0.15f, 0.15f, 1f);
        theme.fillHealthLow = new Color(0.9f, 0.1f, 0.1f, 1f);
        theme.fillXP = new Color(0.4f, 0.8f, 0.3f, 1f);
        theme.fillCombo = new Color(1f, 0.6f, 0.1f, 1f);
        theme.fillProgress = new Color(0.3f, 0.6f, 0.9f, 1f);
        theme.fillWave = new Color(0.6f, 0.3f, 0.5f, 1f);

        theme.buttonNormal = new Color(0.15f, 0.08f, 0.1f, 1f);
        theme.buttonHover = new Color(0.25f, 0.12f, 0.15f, 1f);
        theme.buttonPressed = new Color(0.1f, 0.05f, 0.08f, 1f);
        theme.buttonDisabled = new Color(0.1f, 0.08f, 0.08f, 0.5f);
        theme.buttonAccent = new Color(0.5f, 0.25f, 0.15f, 1f);

        theme.statusDead = new Color(0.4f, 0.1f, 0.1f, 1f);
        theme.statusActive = new Color(0.2f, 0.5f, 0.2f, 1f);
        theme.statusWarning = new Color(0.8f, 0.6f, 0.1f, 1f);
        theme.successColor = new Color(0.2f, 0.5f, 0.2f, 0.95f);
        theme.dangerColor = new Color(0.6f, 0.15f, 0.15f, 0.95f);

        theme.healthLow = new Color(0.9f, 0.2f, 0.2f, 1f);
        theme.healthMedium = new Color(0.9f, 0.6f, 0.2f, 1f);
        theme.healthHigh = new Color(0.3f, 0.8f, 0.3f, 1f);

        theme.accentBrown = new Color(0.5f, 0.35f, 0.15f, 0.95f);
        theme.bloodRed = new Color(0.9f, 0.2f, 0.2f, 1f);
        theme.shadowColor = new Color(0f, 0f, 0f, 0.6f);

        theme.fontSizeXS = 14f;
        theme.fontSizeSM = 18f;
        theme.fontSizeMD = 24f;
        theme.fontSizeLG = 32f;
        theme.fontSizeXL = 40f;
        theme.fontSizeXXL = 48f;

        theme.spacingXS = 4f;
        theme.spacingSM = 8f;
        theme.spacingMD = 16f;
        theme.spacingLG = 24f;
        theme.spacingXL = 32f;

        theme.buttonSizeSmall = 60f;
        theme.buttonSizeMedium = 90f;
        theme.buttonSizeLarge = 120f;
        theme.panelPadding = 20f;
        theme.borderWidth = 3f;
        theme.borderWidthThin = 2f;
        theme.cornerRadius = 4f;

        theme.healthBarHeight = 12f;
        theme.xpBarHeight = 18f;
        theme.progressBarHeight = 8f;
        theme.barInnerPadding = 2f;

        theme.animationFast = 0.15f;
        theme.animationNormal = 0.3f;
        theme.animationSlow = 0.5f;
        theme.pulseSpeed = 20f;
        theme.pulseIntensity = 0.1f;

        return theme;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeOnLoad()
    {
        if (defaultTheme == null)
        {
            defaultTheme = CreateDefaultTheme();
        }
    }
}

public interface IThemeable
{
    void ApplyTheme(UITheme theme);
}

