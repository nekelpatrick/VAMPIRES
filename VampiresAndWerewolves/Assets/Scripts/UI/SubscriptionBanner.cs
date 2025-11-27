using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubscriptionBanner : MonoBehaviour
{
    public static SubscriptionBanner Instance { get; private set; }

    public event Action OnSubscribeClicked;

    [Header("Display")]
    private GameObject bannerRoot;
    private TextMeshProUGUI statusText;
    private Image statusIcon;
    private Button subscribeButton;

    private bool isPremium;
    private DateTime? expiresAt;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CreateBanner(Transform parent)
    {
        bannerRoot = new GameObject("SubscriptionBanner");
        bannerRoot.transform.SetParent(parent);

        RectTransform bannerRect = bannerRoot.AddComponent<RectTransform>();
        bannerRect.anchorMin = new Vector2(0, 1);
        bannerRect.anchorMax = new Vector2(0, 1);
        bannerRect.pivot = new Vector2(0, 1);
        bannerRect.anchoredPosition = new Vector2(20, -90);
        bannerRect.sizeDelta = new Vector2(200, 40);

        Image bannerBg = bannerRoot.AddComponent<Image>();
        bannerBg.color = new Color(0.15f, 0.1f, 0.12f, 0.9f);

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(bannerRoot.transform);

        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(8, 0);
        iconRect.sizeDelta = new Vector2(28, 28);

        statusIcon = iconObj.AddComponent<Image>();
        statusIcon.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bannerRoot.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(42, 5);
        textRect.offsetMax = new Vector2(-10, -5);

        statusText = textObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "FREE";
        statusText.fontSize = 18;
        statusText.fontStyle = FontStyles.Bold;
        statusText.alignment = TextAlignmentOptions.MidlineLeft;
        statusText.color = new Color(0.7f, 0.65f, 0.6f);

        subscribeButton = bannerRoot.AddComponent<Button>();
        subscribeButton.onClick.AddListener(() => OnSubscribeClicked?.Invoke());

        UpdateDisplay();
    }

    public void SetPremiumStatus(bool premium, DateTime? expires = null)
    {
        isPremium = premium;
        expiresAt = expires;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (statusText == null || statusIcon == null) return;

        if (isPremium)
        {
            statusText.text = "[ASHEN ONE]";
            statusText.color = new Color(1f, 0.85f, 0.4f);
            statusIcon.color = new Color(1f, 0.85f, 0.4f);

            if (bannerRoot != null)
            {
                Image bg = bannerRoot.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = new Color(0.2f, 0.15f, 0.1f, 0.95f);
                }
            }
        }
        else
        {
            statusText.text = "FREE";
            statusText.color = new Color(0.7f, 0.65f, 0.6f);
            statusIcon.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);

            if (bannerRoot != null)
            {
                Image bg = bannerRoot.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = new Color(0.15f, 0.1f, 0.12f, 0.9f);
                }
            }
        }
    }

    public bool IsPremium => isPremium;
}

