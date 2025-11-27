using UnityEngine;
using UnityEngine.UI;

public class ClanPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform clanButtonsContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject clanButtonPrefab;

    [Header("Current Clan Display")]
    [SerializeField] private Text currentClanText;
    [SerializeField] private Text currentBonusText;
    [SerializeField] private Image clanBadge;

    private bool isInitialized;

    void Start()
    {
        Initialize();
        if (ClanManager.Instance != null)
        {
            ClanManager.Instance.OnClanChanged += OnClanChanged;
            UpdateCurrentClanDisplay();
        }
    }

    void OnDestroy()
    {
        if (ClanManager.Instance != null)
        {
            ClanManager.Instance.OnClanChanged -= OnClanChanged;
        }
    }

    private void Initialize()
    {
        if (isInitialized) return;

        if (panelRoot == null)
        {
            CreatePanelUI();
        }

        CreateClanButtons();
        isInitialized = true;
    }

    private void CreatePanelUI()
    {
        panelRoot = new GameObject("ClanPanelRoot");
        panelRoot.transform.SetParent(transform);

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var rect = panelRoot.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.1f);
        rect.anchorMax = new Vector2(0.9f, 0.9f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var bg = panelRoot.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.08f, 0.12f, 0.95f);

        var headerObj = new GameObject("Header");
        headerObj.transform.SetParent(panelRoot.transform);
        var headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 0.85f);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.offsetMin = Vector2.zero;
        headerRect.offsetMax = Vector2.zero;

        var headerText = headerObj.AddComponent<Text>();
        headerText.text = "CHOOSE YOUR CLAN";
        headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        headerText.fontSize = 28;
        headerText.alignment = TextAnchor.MiddleCenter;
        headerText.color = new Color(0.9f, 0.8f, 0.6f);

        var currentClanObj = new GameObject("CurrentClan");
        currentClanObj.transform.SetParent(panelRoot.transform);
        var currentRect = currentClanObj.AddComponent<RectTransform>();
        currentRect.anchorMin = new Vector2(0, 0.7f);
        currentRect.anchorMax = new Vector2(1, 0.85f);
        currentRect.offsetMin = Vector2.zero;
        currentRect.offsetMax = Vector2.zero;

        currentClanText = currentClanObj.AddComponent<Text>();
        currentClanText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        currentClanText.fontSize = 20;
        currentClanText.alignment = TextAnchor.MiddleCenter;
        currentClanText.color = Color.white;

        var containerObj = new GameObject("ClanButtons");
        containerObj.transform.SetParent(panelRoot.transform);
        var containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.05f, 0.1f);
        containerRect.anchorMax = new Vector2(0.95f, 0.65f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        var layout = containerObj.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        clanButtonsContainer = containerObj.transform;

        var closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(panelRoot.transform);
        var closeRect = closeObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.4f, 0.02f);
        closeRect.anchorMax = new Vector2(0.6f, 0.08f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;

        var closeBg = closeObj.AddComponent<Image>();
        closeBg.color = new Color(0.3f, 0.1f, 0.1f);

        var closeBtn = closeObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(Hide);

        var closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeObj.transform);
        var closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        var closeText = closeTextObj.AddComponent<Text>();
        closeText.text = "CLOSE";
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.fontSize = 18;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.white;

        panelRoot.SetActive(false);
    }

    private void CreateClanButtons()
    {
        if (clanButtonsContainer == null) return;

        CreateClanButton(ClanType.Nocturnum);
        CreateClanButton(ClanType.Sableheart);
        CreateClanButton(ClanType.Eclipsa);
    }

    private void CreateClanButton(ClanType clan)
    {
        var buttonObj = new GameObject($"Clan_{clan}");
        buttonObj.transform.SetParent(clanButtonsContainer);

        var rect = buttonObj.AddComponent<RectTransform>();
        var layoutElement = buttonObj.AddComponent<LayoutElement>();
        layoutElement.flexibleWidth = 1;
        layoutElement.flexibleHeight = 1;

        var bg = buttonObj.AddComponent<Image>();
        bg.color = ClanManager.Instance?.GetClanColor(clan) ?? Color.gray;

        var button = buttonObj.AddComponent<Button>();
        var clanType = clan;
        button.onClick.AddListener(() => OnClanButtonClicked(clanType));

        var contentObj = new GameObject("Content");
        contentObj.transform.SetParent(buttonObj.transform);
        var contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(10, 10);
        contentRect.offsetMax = new Vector2(-10, -10);

        var layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 10;
        layout.padding = new RectOffset(5, 5, 10, 10);

        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(contentObj.transform);
        var nameText = nameObj.AddComponent<Text>();
        nameText.text = ClanManager.Instance?.GetClanName(clan) ?? clan.ToString();
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 20;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = Color.white;

        var descObj = new GameObject("Description");
        descObj.transform.SetParent(contentObj.transform);
        var descText = descObj.AddComponent<Text>();
        descText.text = ClanManager.Instance?.GetClanDescription(clan) ?? "";
        descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        descText.fontSize = 14;
        descText.alignment = TextAnchor.UpperCenter;
        descText.color = new Color(0.8f, 0.8f, 0.8f);
    }

    private void OnClanButtonClicked(ClanType clan)
    {
        if (ClanManager.Instance != null)
        {
            ClanManager.Instance.SetClan(clan);
        }
    }

    private void OnClanChanged(ClanType clan)
    {
        UpdateCurrentClanDisplay();
    }

    private void UpdateCurrentClanDisplay()
    {
        if (ClanManager.Instance == null) return;

        var currentClan = ClanManager.Instance.CurrentClan;

        if (currentClanText != null)
        {
            if (currentClan == ClanType.None)
            {
                currentClanText.text = "No clan selected";
            }
            else
            {
                var bonuses = ClanManager.Instance.GetCurrentBonuses();
                string bonusText = GetBonusText(bonuses);
                currentClanText.text = $"Current: {ClanManager.Instance.GetClanName(currentClan)}\n{bonusText}";
            }
        }

        if (clanBadge != null)
        {
            clanBadge.color = ClanManager.Instance.GetClanColor(currentClan);
        }
    }

    private string GetBonusText(ClanBonuses bonuses)
    {
        if (bonuses.lifestealBonus > 0)
            return $"+{bonuses.lifestealBonus * 100:0}% Lifesteal";
        if (bonuses.attackSpeedBonus > 0)
            return $"+{bonuses.attackSpeedBonus * 100:0}% Attack Speed";
        if (bonuses.bleedChanceBonus > 0)
            return $"+{bonuses.bleedChanceBonus * 100:0}% Bleed Chance";
        return "";
    }

    public void Show()
    {
        Initialize();
        panelRoot?.SetActive(true);
        UpdateCurrentClanDisplay();
    }

    public void Hide()
    {
        panelRoot?.SetActive(false);
    }

    public void Toggle()
    {
        if (panelRoot != null && panelRoot.activeSelf)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }
}

