using UnityEngine;

[CreateAssetMenu(fileName = "UITheme", menuName = "UI/Theme")]
public class UITheme : ScriptableObject
{
    [Header("Background Colors")]
    public Color backgroundPrimary = new Color(0.04f, 0.02f, 0.05f, 0.95f);
    public Color backgroundSecondary = new Color(0.08f, 0.04f, 0.06f, 0.95f);
    public Color backgroundPanel = new Color(0.05f, 0.03f, 0.05f, 0.95f);
    public Color backgroundDark = new Color(0.02f, 0.01f, 0.03f, 1f);
    public Color backgroundOverlay = new Color(0f, 0f, 0f, 0.7f);

    [Header("Border Colors")]
    public Color borderGold = new Color(0.7f, 0.5f, 0.2f, 0.8f);
    public Color borderBronze = new Color(0.6f, 0.4f, 0.2f, 0.8f);
    public Color borderBlood = new Color(0.6f, 0.15f, 0.15f, 0.8f);
    public Color borderSubtle = new Color(0.5f, 0.35f, 0.2f, 0.6f);

    [Header("Text Colors")]
    public Color textPrimary = Color.white;
    public Color textSecondary = new Color(0.8f, 0.75f, 0.7f, 1f);
    public Color textMuted = new Color(0.6f, 0.55f, 0.5f, 1f);
    public Color textGold = new Color(1f, 0.85f, 0.3f, 1f);
    public Color textBlood = new Color(0.9f, 0.2f, 0.25f, 1f);
    public Color textAccent = new Color(0.9f, 0.8f, 0.6f, 1f);
    public Color textSuccess = new Color(0.4f, 0.8f, 0.3f, 1f);

    [Header("Fill Colors")]
    public Color fillHealth = new Color(0.8f, 0.15f, 0.15f, 1f);
    public Color fillHealthLow = new Color(0.9f, 0.1f, 0.1f, 1f);
    public Color fillXP = new Color(0.4f, 0.8f, 0.3f, 1f);
    public Color fillCombo = new Color(1f, 0.6f, 0.1f, 1f);
    public Color fillProgress = new Color(0.3f, 0.6f, 0.9f, 1f);
    public Color fillWave = new Color(0.6f, 0.3f, 0.5f, 1f);

    [Header("Button Colors")]
    public Color buttonNormal = new Color(0.15f, 0.08f, 0.1f, 1f);
    public Color buttonHover = new Color(0.25f, 0.12f, 0.15f, 1f);
    public Color buttonPressed = new Color(0.1f, 0.05f, 0.08f, 1f);
    public Color buttonDisabled = new Color(0.1f, 0.08f, 0.08f, 0.5f);
    public Color buttonAccent = new Color(0.5f, 0.25f, 0.15f, 1f);

    [Header("Status Colors")]
    public Color statusDead = new Color(0.4f, 0.1f, 0.1f, 1f);
    public Color statusActive = new Color(0.2f, 0.5f, 0.2f, 1f);
    public Color statusWarning = new Color(0.8f, 0.6f, 0.1f, 1f);

    [Header("Typography - Font Sizes")]
    public float fontSizeXS = 14f;
    public float fontSizeSM = 18f;
    public float fontSizeMD = 24f;
    public float fontSizeLG = 32f;
    public float fontSizeXL = 40f;
    public float fontSizeXXL = 48f;

    [Header("Spacing")]
    public float spacingXS = 4f;
    public float spacingSM = 8f;
    public float spacingMD = 16f;
    public float spacingLG = 24f;
    public float spacingXL = 32f;

    [Header("Sizing")]
    public float buttonSizeSmall = 60f;
    public float buttonSizeMedium = 90f;
    public float buttonSizeLarge = 120f;
    public float panelPadding = 20f;
    public float borderWidth = 3f;
    public float borderWidthThin = 2f;
    public float cornerRadius = 4f;

    [Header("Progress Bars")]
    public float healthBarHeight = 12f;
    public float xpBarHeight = 18f;
    public float progressBarHeight = 8f;
    public float barInnerPadding = 2f;

    [Header("Animation")]
    public float animationFast = 0.15f;
    public float animationNormal = 0.3f;
    public float animationSlow = 0.5f;
    public float pulseSpeed = 20f;
    public float pulseIntensity = 0.1f;

    public float GetScaledFontSize(float baseSize)
    {
        if (MobileUIScaler.Instance != null)
        {
            return MobileUIScaler.Instance.GetFontSize(baseSize);
        }
        return baseSize;
    }

    public float GetScaledSize(float baseSize)
    {
        if (MobileUIScaler.Instance != null)
        {
            return MobileUIScaler.Instance.GetButtonSize(baseSize);
        }
        return baseSize;
    }
}

