using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public static class UIFactory
{
    public static GameObject CreatePanel(Transform parent, string name, Vector2 size, 
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition)
    {
        UITheme theme = UIThemeManager.Theme;

        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image bg = panel.AddComponent<Image>();
        bg.color = theme.backgroundPanel;

        AddBorder(panel.transform, theme.borderGold);

        return panel;
    }

    public static GameObject CreateSimplePanel(Transform parent, string name, Vector2 size, Vector2 anchoredPosition)
    {
        return CreatePanel(parent, name, size, 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 
            anchoredPosition);
    }

    public static GameObject CreateTopLeftPanel(Transform parent, string name, Vector2 size, Vector2 offset)
    {
        return CreatePanel(parent, name, size,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            offset);
    }

    public static Button CreateButton(Transform parent, string name, string text, Vector2 size, UnityAction onClick)
    {
        UITheme theme = UIThemeManager.Theme;
        float scaledSize = theme.GetScaledSize(size.x);

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(scaledSize, scaledSize);

        Image bg = btnObj.AddComponent<Image>();
        bg.color = theme.buttonNormal;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = theme.buttonNormal;
        colors.highlightedColor = theme.buttonHover;
        colors.pressedColor = theme.buttonPressed;
        colors.disabledColor = theme.buttonDisabled;
        btn.colors = colors;

        if (onClick != null)
        {
            btn.onClick.AddListener(onClick);
        }

        AddBorder(btnObj.transform, theme.borderGold);

        if (!string.IsNullOrEmpty(text))
        {
            TextMeshProUGUI tmp = CreateText(btnObj.transform, "Label", text, 
                theme.fontSizeLG, theme.textPrimary, FontStyles.Bold);
            RectTransform textRect = tmp.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        return btn;
    }

    public static Button CreateIconButton(Transform parent, string name, string iconText, float size, UnityAction onClick)
    {
        UITheme theme = UIThemeManager.Theme;
        float scaledSize = theme.GetScaledSize(size);
        float fontSize = theme.GetScaledFontSize(theme.fontSizeXL);

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(scaledSize, scaledSize);

        Image bg = btnObj.AddComponent<Image>();
        bg.color = theme.buttonNormal;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = theme.buttonNormal;
        colors.highlightedColor = theme.buttonHover;
        colors.pressedColor = theme.buttonPressed;
        colors.disabledColor = theme.buttonDisabled;
        btn.colors = colors;

        if (onClick != null)
        {
            btn.onClick.AddListener(onClick);
        }

        AddBorder(btnObj.transform, theme.borderGold);

        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(btnObj.transform);
        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.anchoredPosition = new Vector2(2, -2);
        shadowRect.sizeDelta = Vector2.zero;
        shadowObj.transform.SetAsFirstSibling();

        TextMeshProUGUI shadow = shadowObj.AddComponent<TextMeshProUGUI>();
        shadow.text = iconText;
        shadow.fontSize = fontSize;
        shadow.fontStyle = FontStyles.Bold;
        shadow.alignment = TextAlignmentOptions.Center;
        shadow.color = new Color(0, 0, 0, 0.6f);
        shadow.raycastTarget = false;

        TextMeshProUGUI icon = CreateText(btnObj.transform, "Icon", iconText, 
            fontSize, theme.textGold, FontStyles.Bold);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        icon.alignment = TextAlignmentOptions.Center;
        icon.raycastTarget = false;

        AddGlow(btnObj.transform);

        return btn;
    }

    public static Image CreateProgressBar(Transform parent, string name, Color fillColor, 
        float width, float height, bool horizontal = true)
    {
        UITheme theme = UIThemeManager.Theme;

        GameObject container = new GameObject(name);
        container.transform.SetParent(parent);

        RectTransform rect = container.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        Image bg = container.AddComponent<Image>();
        bg.color = theme.backgroundDark;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(container.transform);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = horizontal ? new Vector2(0, 0.5f) : new Vector2(0.5f, 0);
        fillRect.offsetMin = new Vector2(theme.barInnerPadding, theme.barInnerPadding);
        fillRect.offsetMax = new Vector2(-theme.barInnerPadding, -theme.barInnerPadding);

        Image fill = fillObj.AddComponent<Image>();
        fill.color = fillColor;
        fill.type = Image.Type.Filled;
        fill.fillMethod = horizontal ? Image.FillMethod.Horizontal : Image.FillMethod.Vertical;
        fill.fillAmount = 0f;

        return fill;
    }

    public static Image CreateHealthBar(Transform parent, string name, float width)
    {
        UITheme theme = UIThemeManager.Theme;
        return CreateProgressBar(parent, name, theme.fillHealth, width, theme.healthBarHeight);
    }

    public static Image CreateXPBar(Transform parent, string name, float width)
    {
        UITheme theme = UIThemeManager.Theme;
        return CreateProgressBar(parent, name, theme.fillXP, width, theme.xpBarHeight);
    }

    public static TextMeshProUGUI CreateText(Transform parent, string name, string content, 
        float fontSize, Color color, FontStyles style = FontStyles.Normal, float width = 0)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        if (width > 0)
        {
            rect.sizeDelta = new Vector2(width, fontSize * 1.5f);
        }

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.raycastTarget = false;

        return tmp;
    }

    public static TextMeshProUGUI CreateLabel(Transform parent, string name, string content)
    {
        UITheme theme = UIThemeManager.Theme;
        return CreateText(parent, name, content, theme.fontSizeSM, theme.textSecondary);
    }

    public static TextMeshProUGUI CreateTitle(Transform parent, string name, string content)
    {
        UITheme theme = UIThemeManager.Theme;
        return CreateText(parent, name, content, theme.fontSizeLG, theme.textGold, FontStyles.Bold);
    }

    public static TextMeshProUGUI CreateValue(Transform parent, string name, string content)
    {
        UITheme theme = UIThemeManager.Theme;
        return CreateText(parent, name, content, theme.fontSizeMD, theme.textPrimary, FontStyles.Bold);
    }

    public static GameObject CreateBadge(Transform parent, string name, string text, Color bgColor)
    {
        UITheme theme = UIThemeManager.Theme;

        GameObject badge = new GameObject(name);
        badge.transform.SetParent(parent);

        RectTransform rect = badge.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(30, 30);

        Image bg = badge.AddComponent<Image>();
        bg.color = bgColor;

        TextMeshProUGUI tmp = CreateText(badge.transform, "Text", text, 
            theme.fontSizeSM, theme.textPrimary, FontStyles.Bold);
        RectTransform textRect = tmp.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        tmp.alignment = TextAlignmentOptions.Center;

        return badge;
    }

    public static GameObject CreateNotificationBadge(Transform parent, string text)
    {
        UITheme theme = UIThemeManager.Theme;
        return CreateBadge(parent, "NotificationBadge", text, theme.textBlood);
    }

    public static void AddBorder(Transform parent, Color color, float width = -1)
    {
        UITheme theme = UIThemeManager.Theme;
        float borderW = width > 0 ? width : theme.borderWidth;

        string[] sides = { "Top", "Bottom", "Left", "Right" };
        Vector2[] anchorsMin = { new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 0) };
        Vector2[] anchorsMax = { new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        Vector2[] offsets = { 
            new Vector2(0, 0), new Vector2(0, 0), 
            new Vector2(0, 0), new Vector2(0, 0) 
        };
        Vector2[] sizes = { 
            new Vector2(0, borderW), new Vector2(0, borderW), 
            new Vector2(borderW, 0), new Vector2(borderW, 0) 
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject borderObj = new GameObject($"Border{sides[i]}");
            borderObj.transform.SetParent(parent);

            RectTransform rect = borderObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorsMin[i];
            rect.anchorMax = anchorsMax[i];
            rect.offsetMin = offsets[i];
            rect.offsetMax = offsets[i];
            rect.sizeDelta = sizes[i];

            Image img = borderObj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
        }
    }

    public static void AddGlow(Transform parent)
    {
        UITheme theme = UIThemeManager.Theme;

        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(parent);
        glowObj.transform.SetAsFirstSibling();

        RectTransform rect = glowObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(-8, -8);
        rect.offsetMax = new Vector2(8, 8);

        Image glow = glowObj.AddComponent<Image>();
        glow.color = new Color(theme.textGold.r, theme.textGold.g, theme.textGold.b, 0.3f);
        glow.raycastTarget = false;
        glowObj.SetActive(false);
    }

    public static Canvas CreateCanvas(Transform parent, string name, int sortingOrder)
    {
        GameObject canvasObj = new GameObject(name);
        canvasObj.transform.SetParent(parent);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        bool isMobile = MobileUIScaler.Instance != null && MobileUIScaler.Instance.IsMobile;
        scaler.matchWidthOrHeight = isMobile ? 0.5f : 1f;

        canvasObj.AddComponent<GraphicRaycaster>();

        if (MobileUIScaler.Instance != null)
        {
            MobileUIScaler.Instance.ApplyToCanvas(canvas);
        }

        return canvas;
    }

    public static GameObject CreateDimmer(Transform parent, float alpha = 0.7f)
    {
        UITheme theme = UIThemeManager.Theme;

        GameObject dimmer = new GameObject("Dimmer");
        dimmer.transform.SetParent(parent);
        dimmer.transform.SetAsFirstSibling();

        RectTransform rect = dimmer.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = dimmer.AddComponent<Image>();
        img.color = new Color(0, 0, 0, alpha);

        return dimmer;
    }

    public static GameObject CreateCurrencyDisplay(Transform parent, string name, string label, 
        Color iconColor, out TextMeshProUGUI valueText)
    {
        UITheme theme = UIThemeManager.Theme;

        GameObject container = new GameObject(name);
        container.transform.SetParent(parent);

        RectTransform rect = container.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220, 50);

        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = theme.spacingSM;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(container.transform);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(40, 40);

        Image iconBg = iconObj.AddComponent<Image>();
        iconBg.color = iconColor;

        TextMeshProUGUI iconText = CreateText(iconObj.transform, "Symbol", label.Substring(1, 1).ToUpper(), 
            theme.fontSizeMD, theme.backgroundDark, FontStyles.Bold);
        RectTransform iconTextRect = iconText.GetComponent<RectTransform>();
        iconTextRect.anchorMin = Vector2.zero;
        iconTextRect.anchorMax = Vector2.one;
        iconTextRect.offsetMin = Vector2.zero;
        iconTextRect.offsetMax = Vector2.zero;
        iconText.alignment = TextAlignmentOptions.Center;

        valueText = CreateText(container.transform, "Value", "0", 
            theme.fontSizeMD, iconColor, FontStyles.Bold, 150);
        valueText.alignment = TextAlignmentOptions.MidlineLeft;

        return container;
    }
}

