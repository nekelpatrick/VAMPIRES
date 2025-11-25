using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;

    private CombatEntity entity;
    private Transform target;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Initialize(CombatEntity combatEntity)
    {
        entity = combatEntity;
        target = combatEntity.transform;
        entity.OnDamageTaken += HandleDamage;
        UpdateBar();
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;

        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
    }

    void HandleDamage(int damage)
    {
        UpdateBar();
    }

    void UpdateBar()
    {
        if (entity == null || fillImage == null) return;

        float ratio = entity.CurrentHealth / entity.Stats.maxHealth;
        fillImage.fillAmount = ratio;

        if (ratio > 0.6f)
        {
            fillImage.color = healthyColor;
        }
        else if (ratio > 0.3f)
        {
            fillImage.color = damagedColor;
        }
        else
        {
            fillImage.color = criticalColor;
        }
    }

    void OnDestroy()
    {
        if (entity != null)
        {
            entity.OnDamageTaken -= HandleDamage;
        }
    }
}

