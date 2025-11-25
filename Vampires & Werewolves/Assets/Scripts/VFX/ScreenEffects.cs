using UnityEngine;
using UnityEngine.UI;

public class ScreenEffects : MonoBehaviour
{
    public static ScreenEffects Instance { get; private set; }

    [Header("Blood Border")]
    [SerializeField] private float bloodBorderFadeSpeed = 2f;
    [SerializeField] private float maxBloodIntensity = 0.5f;

    [Header("Flash")]
    [SerializeField] private float flashFadeSpeed = 5f;

    private Image bloodBorderImage;
    private Image flashImage;
    private Image vignetteImage;
    private float currentBloodIntensity;
    private float flashIntensity;

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
        CreateOverlayCanvas();
    }

    void CreateOverlayCanvas()
    {
        GameObject canvasObj = new GameObject("ScreenEffectsCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>().blockingObjects = GraphicRaycaster.BlockingObjects.None;

        vignetteImage = CreateFullscreenImage(canvasObj.transform, "Vignette");
        vignetteImage.color = new Color(0, 0, 0, 0);
        CreateVignetteGradient(vignetteImage);

        bloodBorderImage = CreateFullscreenImage(canvasObj.transform, "BloodBorder");
        bloodBorderImage.color = new Color(0.5f, 0, 0, 0);

        flashImage = CreateFullscreenImage(canvasObj.transform, "Flash");
        flashImage.color = new Color(1, 1, 1, 0);
    }

    Image CreateFullscreenImage(Transform parent, string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.raycastTarget = false;

        return img;
    }

    void CreateVignetteGradient(Image image)
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size);

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDist = size * 0.7f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01((dist - maxDist * 0.5f) / (maxDist * 0.5f));
                alpha = alpha * alpha * 0.6f;
                tex.SetPixel(x, y, new Color(0, 0, 0, alpha));
            }
        }

        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
        image.color = Color.white;
    }

    void Update()
    {
        if (bloodBorderImage != null && currentBloodIntensity > 0)
        {
            currentBloodIntensity = Mathf.Lerp(currentBloodIntensity, 0, bloodBorderFadeSpeed * Time.deltaTime);
            Color c = bloodBorderImage.color;
            c.a = currentBloodIntensity;
            bloodBorderImage.color = c;
        }

        if (flashImage != null && flashIntensity > 0)
        {
            flashIntensity = Mathf.Lerp(flashIntensity, 0, flashFadeSpeed * Time.deltaTime);
            Color c = flashImage.color;
            c.a = flashIntensity;
            flashImage.color = c;
        }
    }

    public void FlashBloodBorder(float intensity = 0.3f)
    {
        currentBloodIntensity = Mathf.Min(currentBloodIntensity + intensity, maxBloodIntensity);
    }

    public void FlashWhite(float intensity = 0.3f)
    {
        flashIntensity = intensity;
        if (flashImage != null)
        {
            flashImage.color = new Color(1, 1, 1, intensity);
        }
    }

    public void FlashRed(float intensity = 0.2f)
    {
        flashIntensity = intensity;
        if (flashImage != null)
        {
            flashImage.color = new Color(0.8f, 0.1f, 0.1f, intensity);
        }
    }

    public void FlashOnKill()
    {
        FlashWhite(0.15f);
        FlashBloodBorder(0.2f);
    }

    public void FlashOnCritical()
    {
        FlashWhite(0.1f);
    }

    public void FlashOnThrallHit()
    {
        FlashRed(0.15f);
    }
}

