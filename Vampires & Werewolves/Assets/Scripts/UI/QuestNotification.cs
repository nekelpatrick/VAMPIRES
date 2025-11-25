using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestNotification : MonoBehaviour
{
    public static QuestNotification Instance { get; private set; }

    private GameObject notificationRoot;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI subtitleText;
    private CanvasGroup canvasGroup;

    private float displayTime = 3f;
    private float fadeSpeed = 2f;
    private float timer;
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

    void Start()
    {
        CreateNotification();
        Hide();
    }

    void CreateNotification()
    {
        GameObject canvasObj = new GameObject("QuestNotificationCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 600;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        notificationRoot = new GameObject("NotificationRoot");
        notificationRoot.transform.SetParent(canvasObj.transform);

        RectTransform rect = notificationRoot.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.85f);
        rect.anchorMax = new Vector2(0.5f, 0.85f);
        rect.sizeDelta = new Vector2(500, 100);

        canvasGroup = notificationRoot.AddComponent<CanvasGroup>();

        Image bg = notificationRoot.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.08f, 0.12f, 0.95f);

        GameObject border = CreateImage(notificationRoot.transform, "Border",
            new Color(1f, 0.85f, 0.3f, 0.8f));
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-3, -3);
        borderRect.offsetMax = new Vector2(3, 3);
        border.transform.SetAsFirstSibling();

        titleText = CreateText(notificationRoot.transform, "Title", "QUEST COMPLETE!",
            new Vector2(0.5f, 0.7f), 28);
        titleText.color = new Color(1f, 0.85f, 0.3f);
        titleText.fontStyle = FontStyles.Bold;

        subtitleText = CreateText(notificationRoot.transform, "Subtitle", "Slay the Horde",
            new Vector2(0.5f, 0.3f), 22);
        subtitleText.color = new Color(0.8f, 0.8f, 0.8f);
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

    TextMeshProUGUI CreateText(Transform parent, string name, string content,
        Vector2 anchorPos, int fontSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, anchorPos.y);
        rect.anchorMax = new Vector2(1, anchorPos.y);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 40);

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;

        return text;
    }

    public void ShowCompletion(QuestProgress quest)
    {
        titleText.text = "QUEST COMPLETE!";
        subtitleText.text = quest.questName;

        Show();
    }

    public void ShowClaimed(QuestProgress quest, bool withBonus)
    {
        string bonus = withBonus ? " (2x BONUS!)" : "";
        titleText.text = "REWARD CLAIMED!" + bonus;

        if (quest.bloodShardsReward > 0)
        {
            int amount = withBonus ? quest.bloodShardsReward * 2 : quest.bloodShardsReward;
            subtitleText.text = $"+{amount} Blood Shards";
        }
        else
        {
            int amount = withBonus ? quest.duskenReward * 2 : quest.duskenReward;
            subtitleText.text = $"+{amount} Dusken Coin";
        }

        Show();
    }

    void Show()
    {
        isShowing = true;
        timer = displayTime;
        notificationRoot.SetActive(true);
        canvasGroup.alpha = 1f;

        notificationRoot.transform.localScale = Vector3.one * 0.8f;
    }

    void Hide()
    {
        isShowing = false;
        notificationRoot.SetActive(false);
    }

    void Update()
    {
        if (!isShowing) return;

        float targetScale = timer > 0.5f ? 1f : 0.95f;
        notificationRoot.transform.localScale = Vector3.Lerp(
            notificationRoot.transform.localScale,
            Vector3.one * targetScale,
            Time.deltaTime * 8f
        );

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;

            if (canvasGroup.alpha <= 0)
            {
                Hide();
            }
        }
    }
}

