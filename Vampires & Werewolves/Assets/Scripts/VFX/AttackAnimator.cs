using UnityEngine;

public class AttackAnimator : MonoBehaviour
{
    [Header("Scale Punch")]
    [SerializeField] private float punchScale = 1.3f;
    [SerializeField] private float punchDuration = 0.15f;

    [Header("Lunge")]
    [SerializeField] private float lungeDistance = 0.5f;
    [SerializeField] private float lungeDuration = 0.1f;

    [Header("Hit Flash")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.08f;

    private CombatEntity entity;
    private Renderer meshRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    private bool isAnimating;
    private float animTimer;
    private AnimState currentState;
    private Vector3 lungeEndPosition;

    enum AnimState
    {
        Idle,
        ScaleUp,
        Lunge,
        Return,
        Flash
    }

    void Awake()
    {
        entity = GetComponent<CombatEntity>();
        meshRenderer = GetComponentInChildren<Renderer>();

        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;

        if (entity != null)
        {
            entity.OnAttack += PlayAttackAnimation;
            entity.OnDamageTaken += PlayHitFlash;
        }
    }

    void Update()
    {
        if (!isAnimating) return;

        animTimer -= Time.deltaTime;

        switch (currentState)
        {
            case AnimState.ScaleUp:
                float scaleProgress = 1f - (animTimer / punchDuration);
                float scaleMult = Mathf.Lerp(1f, punchScale, scaleProgress);
                transform.localScale = originalScale * scaleMult;

                if (animTimer <= 0)
                {
                    currentState = AnimState.Lunge;
                    animTimer = lungeDuration;
                }
                break;

            case AnimState.Lunge:
                float lungeProgress = 1f - (animTimer / lungeDuration);
                CombatEntity target = entity.GetTarget();
                if (target != null)
                {
                    Vector3 dir = (target.transform.position - originalPosition).normalized;
                    transform.position = originalPosition + dir * lungeDistance * lungeProgress;
                }

                if (animTimer <= 0)
                {
                    lungeEndPosition = transform.position;
                    currentState = AnimState.Return;
                    animTimer = lungeDuration;
                }
                break;

            case AnimState.Return:
                float returnProgress = 1f - (animTimer / lungeDuration);
                transform.position = Vector3.Lerp(lungeEndPosition, originalPosition, returnProgress);
                transform.localScale = Vector3.Lerp(originalScale * punchScale, originalScale, returnProgress);

                if (animTimer <= 0)
                {
                    transform.localScale = originalScale;
                    transform.position = originalPosition;
                    isAnimating = false;
                    currentState = AnimState.Idle;
                }
                break;

            case AnimState.Flash:
                if (animTimer <= 0)
                {
                    if (meshRenderer != null)
                    {
                        meshRenderer.material.color = originalColor;
                    }
                    isAnimating = false;
                    currentState = AnimState.Idle;
                }
                break;
        }
    }

    public void PlayAttackAnimation()
    {
        if (isAnimating && currentState != AnimState.Flash) return;

        originalPosition = transform.position;
        originalScale = transform.localScale;

        isAnimating = true;
        currentState = AnimState.ScaleUp;
        animTimer = punchDuration;
    }

    void PlayHitFlash(int damage)
    {
        if (meshRenderer == null) return;

        originalColor = meshRenderer.material.color;
        meshRenderer.material.color = flashColor;

        if (!isAnimating)
        {
            isAnimating = true;
            currentState = AnimState.Flash;
            animTimer = flashDuration;
        }
    }

    void OnDestroy()
    {
        if (entity != null)
        {
            entity.OnAttack -= PlayAttackAnimation;
            entity.OnDamageTaken -= PlayHitFlash;
        }
    }
}

