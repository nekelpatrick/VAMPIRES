using UnityEngine;

public class CoinDrop : MonoBehaviour
{
    [SerializeField] private float arcHeight = 1.5f;
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float fadeDelay = 0.3f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float spinSpeed = 720f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float elapsed;
    private bool isAnimating;
    private Renderer meshRenderer;
    private Color originalColor;
    private float totalLifetime;

    void Awake()
    {
        meshRenderer = GetComponentInChildren<Renderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    public void Launch(Vector3 start, Vector3 direction, float spread)
    {
        startPos = start;
        targetPos = start + direction * spread;
        targetPos.y = start.y - 0.3f;

        transform.position = start;
        elapsed = 0f;
        isAnimating = true;
        totalLifetime = duration + fadeDelay + fadeDuration;

        if (meshRenderer != null)
        {
            meshRenderer.material.color = originalColor;
        }

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isAnimating) return;

        elapsed += Time.deltaTime;

        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);

        if (elapsed <= duration)
        {
            float t = elapsed / duration;
            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
            pos.y += arcHeight * 4f * t * (1f - t);
            transform.position = pos;
        }
        else if (elapsed <= duration + fadeDelay)
        {
        }
        else if (elapsed <= totalLifetime)
        {
            float fadeT = (elapsed - duration - fadeDelay) / fadeDuration;
            if (meshRenderer != null)
            {
                Color c = originalColor;
                c.a = 1f - fadeT;
                meshRenderer.material.color = c;
            }
        }
        else
        {
            isAnimating = false;
            gameObject.SetActive(false);
        }
    }

    public void Recycle()
    {
        isAnimating = false;
        gameObject.SetActive(false);
    }
}

