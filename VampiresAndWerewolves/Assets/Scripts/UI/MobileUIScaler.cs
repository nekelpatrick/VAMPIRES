using UnityEngine;
using UnityEngine.UI;

public class MobileUIScaler : MonoBehaviour
{
    public static MobileUIScaler Instance { get; private set; }

    [Header("Scaling Settings")]
    [SerializeField] private float baseDpi = 160f;
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 2.5f;
    [SerializeField] private float mobileMinScale = 1.2f;

    public float ScaleFactor { get; private set; } = 1f;
    public bool IsMobile { get; private set; }

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

        DetectPlatform();
        CalculateScaleFactor();
    }

    void DetectPlatform()
    {
        IsMobile = Application.platform == RuntimePlatform.Android ||
                   Application.platform == RuntimePlatform.IPhonePlayer ||
                   Application.isMobilePlatform;

#if UNITY_EDITOR
        float aspectRatio = (float)Screen.width / Screen.height;
        if (aspectRatio < 1f || Screen.width < 1200)
        {
            IsMobile = true;
        }
#endif
    }

    void CalculateScaleFactor()
    {
        float dpi = Screen.dpi;

        if (dpi <= 0)
        {
            dpi = IsMobile ? 320f : 96f;
        }

        ScaleFactor = dpi / baseDpi;

        if (IsMobile)
        {
            ScaleFactor = Mathf.Max(ScaleFactor, mobileMinScale);
        }

        ScaleFactor = Mathf.Clamp(ScaleFactor, minScale, maxScale);

        Debug.Log($"[MobileUIScaler] DPI: {dpi}, Scale: {ScaleFactor}, IsMobile: {IsMobile}");
    }

    public void ApplyToCanvas(Canvas canvas)
    {
        if (canvas == null) return;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = IsMobile ? 0.5f : 1f;

        float adjustedScale = IsMobile ? ScaleFactor * 1.1f : 1f;
        scaler.referencePixelsPerUnit = 100f / adjustedScale;
    }

    public float GetButtonSize(float baseSize)
    {
        float minMobileSize = 120f;

        if (IsMobile)
        {
            return Mathf.Max(baseSize * ScaleFactor, minMobileSize);
        }

        return baseSize;
    }

    public float GetFontSize(float baseSize)
    {
        if (IsMobile)
        {
            return baseSize * Mathf.Max(1.2f, ScaleFactor * 0.8f);
        }
        return baseSize;
    }

    public Vector2 GetTouchPadding()
    {
        if (IsMobile)
        {
            return new Vector2(20f, 20f) * ScaleFactor;
        }
        return Vector2.zero;
    }
}

