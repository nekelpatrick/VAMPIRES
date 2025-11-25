using UnityEngine;

public class BloodSplatter : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float spreadSpeed = 3f;
    [SerializeField] private float maxScale = 1.5f;

    private float timer;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;
    private bool isFading;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    public void Initialize(Vector3 position, float size, Color color)
    {
        transform.position = position;
        initialScale = Vector3.one * size * 0.3f;
        transform.localScale = initialScale;
        transform.rotation = Quaternion.Euler(90, 0, Random.Range(0f, 360f));

        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        timer = lifetime;
        isFading = false;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (!isFading && transform.localScale.x < initialScale.x * maxScale)
        {
            transform.localScale += Vector3.one * spreadSpeed * Time.deltaTime;
        }

        if (timer <= lifetime * 0.3f)
        {
            isFading = true;
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a -= fadeSpeed * Time.deltaTime;
                spriteRenderer.color = c;

                if (c.a <= 0)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}

