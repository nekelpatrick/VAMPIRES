using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float riseDuration = 0.8f;
    [SerializeField] private float riseHeight = 1.5f;
    [SerializeField] private float xVariance = 0.4f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = new Color(1f, 0.9f, 0.2f);

    private Vector3 startPosition;
    private float elapsed;
    private float randomXOffset;
    private bool isAnimating;
    private bool isCrit;

    void Awake()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }
    }

    public void Show(int damage, Vector3 position, bool isCritical = false)
    {
        transform.position = position;
        startPosition = position;
        elapsed = 0f;
        isAnimating = true;
        isCrit = isCritical;
        randomXOffset = Random.Range(-xVariance, xVariance);

        textMesh.text = NumberFormatter.FormatInt(damage);
        textMesh.color = isCritical ? criticalColor : normalColor;
        textMesh.fontSize = isCritical ? 7f : 5f;

        float scaleMultiplier = isCritical ? 1.5f : 1f;
        transform.localScale = Vector3.one * scaleMultiplier;

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isAnimating) return;

        elapsed += Time.deltaTime;
        float t = elapsed / riseDuration;

        if (t >= 1f)
        {
            isAnimating = false;
            gameObject.SetActive(false);
            return;
        }

        Vector3 pos = startPosition;
        pos.y += riseHeight * EaseOutCubic(t);
        pos.x += randomXOffset + Mathf.Sin(t * Mathf.PI * 2f) * 0.15f;
        transform.position = pos;

        float alpha = fadeCurve.Evaluate(t);
        Color c = textMesh.color;
        c.a = alpha;
        textMesh.color = c;

        if (isCrit)
        {
            float pulse = 1f + Mathf.Sin(elapsed * 15f) * 0.1f;
            transform.localScale = Vector3.one * 1.5f * pulse;
        }
    }

    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    public void Recycle()
    {
        isAnimating = false;
        gameObject.SetActive(false);
    }
}
