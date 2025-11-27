using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelUpVFX : MonoBehaviour
{
    public static LevelUpVFX Instance { get; private set; }

    [Header("Particle Settings")]
    [SerializeField] private int particleCount = 30;
    [SerializeField] private float particleLifetime = 1.5f;
    [SerializeField] private float particleSpeed = 3f;
    [SerializeField] private float particleSpread = 2f;

    [Header("Slow Motion")]
    [SerializeField] private float slowMoDuration = 0.4f;
    [SerializeField] private float slowMoScale = 0.3f;

    private Canvas worldCanvas;
    private GameObject particleContainer;

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

        CreateWorldCanvas();
    }

    void Start()
    {
        ThrallController.OnLevelUp += OnThrallLevelUp;
    }

    void CreateWorldCanvas()
    {
        GameObject canvasObj = new GameObject("LevelUpCanvas");
        canvasObj.transform.SetParent(transform);

        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 150;

        RectTransform rect = canvasObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(10, 10);

        particleContainer = new GameObject("ParticleContainer");
        particleContainer.transform.SetParent(canvasObj.transform);
    }

    void OnThrallLevelUp(int newLevel)
    {
        ThrallController thrall = CombatManager.Instance?.GetThrall();
        if (thrall == null) return;

        Vector3 position = thrall.transform.position + Vector3.up * 1.5f;
        StartCoroutine(PlayLevelUpSequence(position, newLevel));
    }

    IEnumerator PlayLevelUpSequence(Vector3 position, int level)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = slowMoScale;

        SpawnParticleBurst(position);

        ScreenEffects.Instance?.FlashGold(0.4f);

        SpawnLevelUpText(position, level);

        CameraEffects.Instance?.Shake(0.25f);

        yield return new WaitForSecondsRealtime(slowMoDuration);

        Time.timeScale = originalTimeScale;
    }

    void SpawnParticleBurst(Vector3 center)
    {
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = CreateParticle();
            particle.transform.position = center;

            float angle = (float)i / particleCount * 360f;
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
            direction += new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f),
                0
            );

            StartCoroutine(AnimateParticle(particle, direction));
        }
    }

    GameObject CreateParticle()
    {
        GameObject obj = new GameObject("LevelParticle");
        obj.transform.SetParent(particleContainer.transform);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.sortingOrder = 160;

        float colorVariation = Random.Range(0.9f, 1.1f);
        sr.color = new Color(
            1f * colorVariation,
            0.85f * colorVariation,
            0.2f,
            1f
        );

        float size = Random.Range(0.15f, 0.35f);
        obj.transform.localScale = Vector3.one * size;

        return obj;
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 1f - Mathf.Clamp01(dist / (size / 2f));
                alpha = alpha * alpha;
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    IEnumerator AnimateParticle(GameObject particle, Vector3 direction)
    {
        float elapsed = 0f;
        Vector3 startPos = particle.transform.position;
        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        Color startColor = sr.color;
        Vector3 startScale = particle.transform.localScale;

        float speed = particleSpeed * Random.Range(0.8f, 1.2f);
        float spread = particleSpread * Random.Range(0.7f, 1.3f);

        while (elapsed < particleLifetime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / particleLifetime;

            particle.transform.position = startPos + direction * spread * t;
            particle.transform.position += Vector3.up * speed * t * (1f - t);

            sr.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * (1f - t));
            particle.transform.localScale = startScale * (1f - t * 0.5f);

            yield return null;
        }

        Destroy(particle);
    }

    void SpawnLevelUpText(Vector3 position, int level)
    {
        GameObject textObj = new GameObject("LevelUpText");
        textObj.transform.SetParent(worldCanvas.transform);
        textObj.transform.position = position + Vector3.up * 0.5f;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = $"LEVEL {level}!";
        tmp.fontSize = 8;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.9f, 0.3f);
        tmp.sortingOrder = 170;

        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color(0.3f, 0.1f, 0f, 1f);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(5, 2);

        StartCoroutine(AnimateLevelUpText(textObj));
    }

    IEnumerator AnimateLevelUpText(GameObject textObj)
    {
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startPos = textObj.transform.position;
        Vector3 startScale = Vector3.one * 0.5f;
        TextMeshPro tmp = textObj.GetComponent<TextMeshPro>();

        textObj.transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            textObj.transform.position = startPos + Vector3.up * t * 2f;

            float scaleT = t < 0.2f ? t / 0.2f : 1f;
            textObj.transform.localScale = Vector3.Lerp(startScale, Vector3.one, scaleT);

            float alpha = t < 0.7f ? 1f : 1f - ((t - 0.7f) / 0.3f);
            Color c = tmp.color;
            c.a = alpha;
            tmp.color = c;

            yield return null;
        }

        Destroy(textObj);
    }

    void OnDestroy()
    {
        ThrallController.OnLevelUp -= OnThrallLevelUp;
    }
}

