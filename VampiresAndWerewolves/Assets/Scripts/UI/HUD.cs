using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI duskenCoinText;
    [SerializeField] private TextMeshProUGUI bloodShardsText;

    [Header("Wave Display")]
    [SerializeField] private TextMeshProUGUI waveText;

    [Header("Thrall Health")]
    [SerializeField] private TextMeshProUGUI thrallHealthText;

    private CurrencyManager currencyManager;
    private CombatManager combatManager;
    private HordeSpawner hordeSpawner;
    private ThrallController thrall;

    void Start()
    {
        currencyManager = CurrencyManager.Instance;
        combatManager = CombatManager.Instance;
        hordeSpawner = FindFirstObjectByType<HordeSpawner>();

        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
            UpdateCurrencyDisplay(currencyManager.DuskenCoin, currencyManager.BloodShards);
        }

        if (hordeSpawner != null)
        {
            hordeSpawner.OnWaveStarted += UpdateWaveDisplay;
        }

        if (combatManager != null)
        {
            thrall = combatManager.GetThrall();
            if (thrall != null)
            {
                thrall.OnDamageTaken += _ => UpdateThrallHealth();
            }
        }

        UpdateWaveDisplay(1);
    }

    void Update()
    {
        if (thrall == null && combatManager != null)
        {
            thrall = combatManager.GetThrall();
            if (thrall != null)
            {
                thrall.OnDamageTaken += _ => UpdateThrallHealth();
                UpdateThrallHealth();
            }
        }
    }

    void UpdateCurrencyDisplay(int dusken, int shards)
    {
        if (duskenCoinText != null)
        {
            duskenCoinText.text = $"[DUSKEN COIN]: {dusken}";
        }

        if (bloodShardsText != null)
        {
            bloodShardsText.text = $"[BLOOD SHARDS]: {shards}";
        }
    }

    void UpdateWaveDisplay(int wave)
    {
        if (waveText != null)
        {
            waveText.text = $"WAVE {wave}";
        }
    }

    void UpdateThrallHealth()
    {
        if (thrallHealthText == null || thrall == null) return;

        float current = thrall.CurrentHealth;
        float max = thrall.Stats.maxHealth;
        thrallHealthText.text = $"[THRALL] HP: {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

    void OnDestroy()
    {
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
        }

        if (hordeSpawner != null)
        {
            hordeSpawner.OnWaveStarted -= UpdateWaveDisplay;
        }
    }
}

