using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    public static CameraEffects Instance { get; private set; }

    [Header("Screen Shake")]
    [SerializeField] private float shakeDecay = 5f;
    [SerializeField] private float maxShakeIntensity = 0.3f;

    [Header("Slow Motion")]
    [SerializeField] private float slowMoScale = 0.2f;
    [SerializeField] private float slowMoDuration = 0.3f;

    [Header("Hit Pause")]
    [SerializeField] private float hitPauseDuration = 0.05f;

    private Vector3 originalPosition;
    private float shakeIntensity;
    private float slowMoTimer;
    private float hitPauseTimer;
    private bool isHitPaused;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        originalPosition = transform.position;
    }

    void Update()
    {
        if (hitPauseTimer > 0)
        {
            hitPauseTimer -= Time.unscaledDeltaTime;
            if (hitPauseTimer <= 0)
            {
                Time.timeScale = 1f;
                isHitPaused = false;
            }
        }

        if (slowMoTimer > 0 && !isHitPaused)
        {
            slowMoTimer -= Time.unscaledDeltaTime;
            if (slowMoTimer <= 0)
            {
                Time.timeScale = 1f;
            }
        }

        if (shakeIntensity > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            shakeOffset.z = 0;
            transform.position = originalPosition + shakeOffset;
            shakeIntensity = Mathf.Lerp(shakeIntensity, 0, shakeDecay * Time.unscaledDeltaTime);

            if (shakeIntensity < 0.01f)
            {
                shakeIntensity = 0;
                transform.position = originalPosition;
            }
        }
    }

    public void Shake(float intensity)
    {
        shakeIntensity = Mathf.Min(shakeIntensity + intensity, maxShakeIntensity);
    }

    public void ShakeOnDamage(int damage)
    {
        float intensity = Mathf.Clamp(damage / 100f, 0.02f, 0.15f);
        Shake(intensity);
    }

    public void ShakeOnKill()
    {
        Shake(0.12f);
    }

    public void ShakeOnCritical()
    {
        Shake(0.08f);
    }

    public void TriggerSlowMo(float duration = -1)
    {
        if (isHitPaused) return;

        slowMoTimer = duration > 0 ? duration : slowMoDuration;
        Time.timeScale = slowMoScale;
    }

    public void TriggerHitPause()
    {
        hitPauseTimer = hitPauseDuration;
        Time.timeScale = 0f;
        isHitPaused = true;
    }

    public void TriggerBossKillEffect()
    {
        Shake(0.25f);
        TriggerSlowMo(0.5f);
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}

