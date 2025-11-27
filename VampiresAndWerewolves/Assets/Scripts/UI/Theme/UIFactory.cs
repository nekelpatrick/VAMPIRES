using System;
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
        panel.transform.SetParent(parent, false);

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

    public static GameObject CreatePanel(Transform parent, string name)
    {
        return CreatePanel(parent, name, Vector2.zero,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
    }

    public static GameObject CreatePanel(Transform parent, string name, float width, float height)
    {
        return CreatePanel(parent, name, new Vector2(width, height),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
    }

    public static GameObject CreatePanel(string name, Transform parent)
    {
        return CreatePanel(parent, name);
    }

    public static GameObject CreatePanel(string name, Transform parent, Vector2 size)
    {
        return CreatePanel(parent, name, size,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
    }

    public static GameObject CreatePanel(string name, Transform parent, float width, float height)
    {
        return CreatePanel(parent, name, width, height);
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

    public static Button CreateButton(Transform parent, string name, string text, Vector2 size, UnityAction onClick = null)
    {
        UITheme theme = UIThemeManager.Theme;
        float rawWidth = size.x > 0.01f ? size.x : (theme != null ? theme.buttonSizeMedium : 100f);
        float rawHeight = size.y > 0.01f ? size.y : rawWidth * 0.6f;
        float scaledWidth = theme != null ? theme.GetScaledSize(rawWidth) : rawWidth;
        float scaledHeight = theme != null ? theme.GetScaledSize(rawHeight) : rawHeight;

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(scaledWidth, scaledHeight);

        Image bg = btnObj.AddComponent<Image>();
        if (theme != null)
        {
            bg.color = theme.buttonNormal;
        }

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        if (theme != null)
        {
            colors.normalColor = theme.buttonNormal;
            colors.highlightedColor = theme.buttonHover;
            colors.pressedColor = theme.buttonPressed;
            colors.disabledColor = theme.buttonDisabled;
        }
        btn.colors = colors;

        if (onClick != null)
        {
            btn.onClick.AddListener(onClick);
        }

        AddBorder(btnObj.transform, theme.borderGold);

        if (!string.IsNullOrEmpty(text))
        {
            TextMeshProUGUI tmp = CreateText(btnObj.transform, "Label", text, 
                theme != null ? theme.fontSizeLG : 30f,
                theme != null ? theme.textPrimary : Color.white, FontStyles.Bold);
            RectTransform textRect = tmp.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        return btn;
    }

    public static Button CreateIconButton(Transform parent, string name, string iconText, UnityAction onClick = null)
    {
        UITheme theme = UIThemeManager.Theme;
        float defaultSize = theme != null ? theme.buttonSizeSmall : 64f;
        return CreateIconButton(parent, name, iconText, defaultSize, onClick);
    }

    public static Button CreateIconButton(string name, Transform parent, string iconText, Action onClick)
    {
        UnityAction handler = onClick != null ? new UnityAction(onClick) : null;
        return CreateIconButton(parent, name, iconText, handler);
    }

    public static Button CreateButton(Transform parent, string name, string text, float width, float height, UnityAction onClick = null)
    {
        return CreateButton(parent, name, text, new Vector2(width, height), onClick);
    }

    public static Button CreateButton(Transform parent, string name, string text, UnityAction onClick = null)
    {
        UITheme theme = UIThemeManager.Theme;
        float defaultSize = theme != null ? theme.buttonSizeMedium : 100f;
        return CreateButton(parent, name, text, new Vector2(defaultSize, defaultSize * 0.6f), onClick);
    }

    public static GameObject CreateButton(string name, Transform parent, string text, Action onClick)
    {
        UnityAction handler = onClick != null ? new UnityAction(onClick) : null;
        Button btn = CreateButton(parent, name, text, handler);
        return btn.gameObject;
    }

    public static Button CreateButton(string name, Transform parent, string text, float width, float height, Action onClick)
    {
        UnityAction handler = onClick != null ? new UnityAction(onClick) : null;
        return CreateButton(parent, name, text, new Vector2(width, height), handler);
    }

    public static Button CreateIconButton(Transform parent, string name, string iconText, float size, UnityAction onClick = null)
    {
        UITheme theme = UIThemeManager.Theme;
        float scaledSize = theme != null ? theme.GetScaledSize(size) : size;
        float fontSize = theme != null ? theme.GetScaledFontSize(theme.fontSizeXL) : size * 0.6f;

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(scaledSize, scaledSize);

        Image bg = btnObj.AddComponent<Image>();
        if (theme != null)
        {
            bg.color = theme.buttonNormal;
        }

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        if (theme != null)
        {
            colors.normalColor = theme.buttonNormal;
            colors.highlightedColor = theme.buttonHover;
            colors.pressedColor = theme.buttonPressed;
            colors.disabledColor = theme.buttonDisabled;
        }
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
            fontSize, theme != null ? theme.textGold : Color.yellow, FontStyles.Bold);
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
        textObj.transform.SetParent(parent, false);

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

    public static TextMeshProUGUI CreateText(string name, Transform parent, string content, float fontSize)
    {
        UITheme theme = UIThemeManager.Theme;
        Color color = theme != null ? theme.textPrimary : Color.white;
        return CreateText(parent, name, content, fontSize, color);
    }

    public static TextMeshProUGUI CreateText(string name, Transform parent, string content, float fontSize, Color color)
    {
        return CreateText(parent, name, content, fontSize, color, FontStyles.Normal);
    }

    public static TextMeshProUGUI CreateText(string name, Transform parent, string content, float fontSize, Color color, FontStyles style)
    {
        return CreateText(parent, name, content, fontSize, color, style, 0);
    }

    public static TextMeshProUGUI CreateLabel(Transform parent, string name, string content)
    {
        UITheme theme = UIThemeManager.Theme;
        return CreateText(parent, name, content, theme.fontSizeSM, theme.textSecondary);
    }

    public static TextMeshProUGUI CreateLabel(string name, Transform parent, string content)
    {
        return CreateLabel(parent, name, content);
    }

    public static TextMeshProUGUI CreateTitle(Transform parent, string name, string content)
    {
        UITheme theme = UIThemeManager.Theme;
        return CreateText(parent, name, content, theme.fontSizeLG, theme.textGold, FontStyles.Bold);
    }

    public static TextMeshProUGUI CreateTitle(string name, Transform parent, string content)
    {
        return CreateTitle(parent, name, content);
    }

    public static TextMeshProUGUI CreateValue(Transform parent, string name, string content)
    {
        UITheme theme = UIThemeManager.Theme;
        return CreateText(parent, name, content, theme.fontSizeMD, theme.textPrimary, FontStyles.Bold);
    }

    public static TextMeshProUGUI CreateValue(string name, Transform parent, string content)
    {
        return CreateValue(parent, name, content);
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
        canvasObj.transform.SetParent(parent, false);

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

    public static GameObject CreateCanvas(string name, Transform parent, int sortingOrder)
    {
        Canvas canvas = CreateCanvas(parent, name, sortingOrder);
        return canvas != null ? canvas.gameObject : null;
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

    /// <summary>
    /// Creates a button with an accent color, typically for action buttons like "WATCH AD" or "BUY NOW"
    /// </summary>
    public static Button CreateAccentButton(Transform parent, string name, string text, 
        Vector2 size, Color accentColor, UnityAction onClick)
    {
        UITheme theme = UIThemeManager.Theme;
        float scaledWidth = theme.GetScaledSize(size.x);
        float scaledHeight = theme.GetScaledSize(size.y);
        float fontSize = theme.GetScaledFontSize(theme.fontSizeMD);

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(scaledWidth, scaledHeight);

        Image bg = btnObj.AddComponent<Image>();
        bg.color = accentColor;

        Button btn = btnObj.AddComponent<Button>();
        
        // Create color variants for button states
        Color hoverColor = Color.Lerp(accentColor, Color.white, 0.2f);
        Color pressedColor = Color.Lerp(accentColor, Color.black, 0.2f);
        Color disabledColor = new Color(accentColor.r * 0.5f, accentColor.g * 0.5f, accentColor.b * 0.5f, 0.5f);
        
        ColorBlock colors = btn.colors;
        colors.normalColor = accentColor;
        colors.highlightedColor = hoverColor;
        colors.pressedColor = pressedColor;
        colors.disabledColor = disabledColor;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

        if (onClick != null)
        {
            btn.onClick.AddListener(onClick);
        }

        // Add glow border for accent buttons
        AddBorder(btnObj.transform, Color.Lerp(accentColor, Color.white, 0.4f), theme.borderWidth);

        // Create text
        if (!string.IsNullOrEmpty(text))
        {
            // Shadow
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(btnObj.transform);
            RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.anchoredPosition = new Vector2(1, -1);
            shadowRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI shadow = shadowObj.AddComponent<TextMeshProUGUI>();
            shadow.text = text;
            shadow.fontSize = fontSize;
            shadow.fontStyle = FontStyles.Bold;
            shadow.alignment = TextAlignmentOptions.Center;
            shadow.color = new Color(0, 0, 0, 0.5f);
            shadow.raycastTarget = false;

            // Main text
            TextMeshProUGUI tmp = CreateText(btnObj.transform, "Label", text, 
                fontSize, theme.textPrimary, FontStyles.Bold);
            RectTransform textRect = tmp.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        return btn;
    }

    /// <summary>
    /// Creates a tab button for tab-based navigation (e.g., shop tabs)
    /// </summary>
    public static Button CreateTabButton(Transform parent, string name, string text, 
        Vector2 size, bool isSelected, UnityAction onClick, out Image backgroundImage, out TextMeshProUGUI labelText)
    {
        UITheme theme = UIThemeManager.Theme;
        float scaledWidth = theme.GetScaledSize(size.x);
        float scaledHeight = theme.GetScaledSize(size.y);
        float fontSize = theme.GetScaledFontSize(theme.fontSizeSM);

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(scaledWidth, scaledHeight);

        backgroundImage = btnObj.AddComponent<Image>();
        backgroundImage.color = isSelected ? theme.buttonNormal : theme.backgroundDark;

        Button btn = btnObj.AddComponent<Button>();
        
        ColorBlock colors = btn.colors;
        colors.normalColor = isSelected ? theme.buttonNormal : theme.backgroundDark;
        colors.highlightedColor = theme.buttonHover;
        colors.pressedColor = theme.buttonPressed;
        colors.disabledColor = theme.buttonDisabled;
        btn.colors = colors;

        if (onClick != null)
        {
            btn.onClick.AddListener(onClick);
        }

        // Border only on selected tab
        if (isSelected)
        {
            AddBorder(btnObj.transform, theme.borderGold, theme.borderWidth);
        }
        else
        {
            AddBorder(btnObj.transform, theme.borderSubtle, theme.borderWidth);
        }

        // Create text
        labelText = CreateText(btnObj.transform, "Label", text, 
            fontSize, isSelected ? theme.textGold : theme.textMuted, FontStyles.Bold);
        RectTransform textRect = labelText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        labelText.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    /// <summary>
    /// Helper to update tab selection state
    /// </summary>
    public static void SetTabSelected(Button tabButton, Image background, TextMeshProUGUI label, bool isSelected)
    {
        UITheme theme = UIThemeManager.Theme;
        
        background.color = isSelected ? theme.buttonNormal : theme.backgroundDark;
        label.color = isSelected ? theme.textGold : theme.textMuted;
        
        // Update button colors
        ColorBlock colors = tabButton.colors;
        colors.normalColor = isSelected ? theme.buttonNormal : theme.backgroundDark;
        tabButton.colors = colors;

        // Update borders (destroy old and add new)
        Transform parent = tabButton.transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child.name.StartsWith("Border"))
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
        
        AddBorder(parent, isSelected ? theme.borderGold : theme.borderSubtle, theme.borderWidth);
    }

    /// <summary>
    /// Creates a simple row container with horizontal layout
    /// </summary>
    public static GameObject CreateRow(Transform parent, string name, float height, float spacing = -1)
    {
        UITheme theme = UIThemeManager.Theme;
        float rowSpacing = spacing > 0 ? spacing : theme.spacingMD;

        GameObject row = new GameObject(name);
        row.transform.SetParent(parent);

        RectTransform rect = row.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, height);

        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = rowSpacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        return row;
    }

    /// <summary>
    /// Creates a vertical container with vertical layout
    /// </summary>
    public static GameObject CreateColumn(Transform parent, string name, float width, float spacing = -1)
    {
        UITheme theme = UIThemeManager.Theme;
        float colSpacing = spacing > 0 ? spacing : theme.spacingMD;

        GameObject col = new GameObject(name);
        col.transform.SetParent(parent);

        RectTransform rect = col.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 0);

        VerticalLayoutGroup layout = col.AddComponent<VerticalLayoutGroup>();
        layout.spacing = colSpacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        return col;
    }

    /// <summary>
    /// Creates a scrollable content area
    /// </summary>
    public static ScrollRect CreateScrollView(Transform parent, string name, Vector2 size, bool horizontal = false, bool vertical = true)
    {
        UITheme theme = UIThemeManager.Theme;

        GameObject scrollObj = new GameObject(name);
        scrollObj.transform.SetParent(parent);

        RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
        scrollRect.sizeDelta = size;

        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = Color.clear;

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = horizontal;
        scroll.vertical = vertical;
        scroll.scrollSensitivity = 30f;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform);

        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        viewport.AddComponent<Image>().color = Color.clear;
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);

        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);

        // Layout on content
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = theme.spacingMD;
        layout.padding = new RectOffset((int)theme.spacingMD, (int)theme.spacingMD, (int)theme.spacingMD, (int)theme.spacingMD);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = contentRect;

        return scroll;
    }

    /// <summary>
    /// Creates a divider line
    /// </summary>
    public static GameObject CreateDivider(Transform parent, string name, float width, bool horizontal = true)
    {
        UITheme theme = UIThemeManager.Theme;

        GameObject divider = new GameObject(name);
        divider.transform.SetParent(parent);

        RectTransform rect = divider.AddComponent<RectTransform>();
        rect.sizeDelta = horizontal ? new Vector2(width, 2) : new Vector2(2, width);

        Image img = divider.AddComponent<Image>();
        img.color = theme.borderSubtle;

        return divider;
    }
}

