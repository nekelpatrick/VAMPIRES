using UnityEngine;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [SerializeField] private Transform fillTransform;
    [SerializeField] private Renderer fillRenderer;
    [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.9f, 0.2f);
    [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private bool hideWhenFull = false;

    private CombatEntity entity;
    private Vector3 originalFillScale;
    private bool isInitialized;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        FindReferences();
    }

    void FindReferences()
    {
        entity = GetComponentInParent<CombatEntity>();

        if (fillTransform == null)
        {
            Transform fill = transform.Find("Fill");
            if (fill != null) fillTransform = fill;
        }

        if (fillRenderer == null && fillTransform != null)
        {
            fillRenderer = fillTransform.GetComponent<Renderer>();
        }

        if (fillTransform != null)
        {
            originalFillScale = fillTransform.localScale;
        }

        if (entity != null)
        {
            entity.OnDamageTaken += OnHealthChanged;
            isInitialized = true;
            UpdateBar();
        }
    }

    void LateUpdate()
    {
        if (!isInitialized) return;

        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) transform.rotation = mainCamera.transform.rotation;

        if (hideWhenFull && entity != null)
        {
            bool shouldHide = entity.CurrentHealth >= entity.Stats.maxHealth;
            if (gameObject.activeSelf != !shouldHide)
            {
                gameObject.SetActive(!shouldHide);
            }
        }
    }

    void OnHealthChanged(int damage)
    {
        UpdateBar();
    }

    void UpdateBar()
    {
        if (entity == null || fillTransform == null) return;

        float healthPercent = entity.CurrentHealth / entity.Stats.maxHealth;
        healthPercent = Mathf.Clamp01(healthPercent);

        Vector3 scale = originalFillScale;
        scale.x *= healthPercent;
        fillTransform.localScale = scale;

        float xOffset = (1f - healthPercent) * originalFillScale.x * 0.5f;
        Vector3 pos = fillTransform.localPosition;
        pos.x = -xOffset;
        fillTransform.localPosition = pos;

        if (fillRenderer != null)
        {
            Color targetColor = healthPercent <= lowHealthThreshold ? lowHealthColor : fullHealthColor;
            if (healthPercent <= lowHealthThreshold)
            {
                float pulse = 0.7f + Mathf.Abs(Mathf.Sin(Time.time * 5f)) * 0.3f;
                targetColor *= pulse;
            }
            fillRenderer.material.color = targetColor;
        }
    }

    void OnDestroy()
    {
        if (entity != null)
        {
            entity.OnDamageTaken -= OnHealthChanged;
        }
    }
}

