using System;
using System.Collections.Generic;
using UnityEngine;

public class AdRewardManager : MonoBehaviour
{
    public static AdRewardManager Instance { get; private set; }

    public event Action<AdType, float> OnRewardGranted;
    public event Action<AdType> OnAdOffered;
    public event Action OnDamageBoostStarted;
    public event Action OnDamageBoostEnded;

    [Header("Reward Settings")]
    [SerializeField] private float doubleLootMultiplier = 2f;
    [SerializeField] private float damageBoostMultiplier = 2f;
    [SerializeField] private float damageBoostDuration = 60f;
    [SerializeField] private float offlineMultiplier = 2f;

    [Header("Cooldowns (seconds)")]
    [SerializeField] private float doubleLootCooldown = 120f;
    [SerializeField] private float damageBoostCooldown = 300f;

    [Header("Offer Frequency")]
    [SerializeField] private int wavesPerLootOffer = 5;

    private Dictionary<AdType, float> lastAdTime = new Dictionary<AdType, float>();
    private bool isDamageBoostActive;
    private float damageBoostTimer;
    private int pendingDoubleLootWave;
    private bool hasPendingOfflineReward;
    private int pendingOfflineEarnings;

    private AdService adService;
    private CurrencyManager currencyManager;
    private CombatManager combatManager;
    private HordeSpawner hordeSpawner;

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

        foreach (AdType type in Enum.GetValues(typeof(AdType)))
        {
            lastAdTime[type] = -9999f;
        }
    }

    void Start()
    {
        adService = AdService.Instance;
        currencyManager = CurrencyManager.Instance;
        combatManager = CombatManager.Instance;
        hordeSpawner = UnityEngine.Object.FindFirstObjectByType<HordeSpawner>();

        if (hordeSpawner != null)
        {
            hordeSpawner.OnWaveCompleted += OnWaveCompleted;
        }

        if (combatManager != null)
        {
            combatManager.OnThrallDied += OnThrallDied;
        }
    }

    void Update()
    {
        if (isDamageBoostActive)
        {
            damageBoostTimer -= Time.deltaTime;
            if (damageBoostTimer <= 0)
            {
                EndDamageBoost();
            }
        }
    }

    void OnWaveCompleted(int wave)
    {
        if (wave % wavesPerLootOffer == 0 && CanOfferAd(AdType.DoubleLoot))
        {
            pendingDoubleLootWave = wave;
            OnAdOffered?.Invoke(AdType.DoubleLoot);
        }
    }

    void OnThrallDied()
    {
        OnAdOffered?.Invoke(AdType.InstantRevive);
    }

    public bool CanOfferAd(AdType type)
    {
        if (adService == null || !adService.IsAdReady(type))
            return false;

        float cooldown = GetCooldown(type);
        float timeSinceLastAd = Time.time - lastAdTime[type];

        return timeSinceLastAd >= cooldown;
    }

    float GetCooldown(AdType type)
    {
        return type switch
        {
            AdType.DoubleLoot => doubleLootCooldown,
            AdType.InstantRevive => 0f,
            AdType.DamageBoost => damageBoostCooldown,
            AdType.DoubleOffline => 0f,
            _ => 0f
        };
    }

    public void RequestDoubleLoot(Action<bool> callback)
    {
        if (!CanOfferAd(AdType.DoubleLoot))
        {
            callback?.Invoke(false);
            return;
        }

        adService.ShowRewardedAd(
            AdType.DoubleLoot,
            () =>
            {
                lastAdTime[AdType.DoubleLoot] = Time.time;
                GrantDoubleLoot();
                callback?.Invoke(true);
            },
            () => callback?.Invoke(false)
        );
    }

    void GrantDoubleLoot()
    {
        int bonusCoins = Mathf.RoundToInt(pendingDoubleLootWave * 50 * (doubleLootMultiplier - 1));
        currencyManager?.AddDuskenCoin(bonusCoins);

        OnRewardGranted?.Invoke(AdType.DoubleLoot, bonusCoins);
        Debug.Log($"[AdRewardManager] Double loot granted: +{bonusCoins} Dusken Coin");
    }

    public void RequestInstantRevive(Action<bool> callback)
    {
        adService.ShowRewardedAd(
            AdType.InstantRevive,
            () =>
            {
                lastAdTime[AdType.InstantRevive] = Time.time;
                GrantInstantRevive();
                callback?.Invoke(true);
            },
            () => callback?.Invoke(false)
        );
    }

    void GrantInstantRevive()
    {
        ThrallController thrall = combatManager?.GetThrall();
        if (thrall != null)
        {
            thrall.Revive();
        }

        OnRewardGranted?.Invoke(AdType.InstantRevive, 1);
        Debug.Log("[AdRewardManager] Instant revive granted via ad");
    }

    public void RequestDamageBoost(Action<bool> callback)
    {
        if (!CanOfferAd(AdType.DamageBoost) || isDamageBoostActive)
        {
            callback?.Invoke(false);
            return;
        }

        adService.ShowRewardedAd(
            AdType.DamageBoost,
            () =>
            {
                lastAdTime[AdType.DamageBoost] = Time.time;
                StartDamageBoost();
                callback?.Invoke(true);
            },
            () => callback?.Invoke(false)
        );
    }

    void StartDamageBoost()
    {
        isDamageBoostActive = true;
        damageBoostTimer = damageBoostDuration;

        OnDamageBoostStarted?.Invoke();
        OnRewardGranted?.Invoke(AdType.DamageBoost, damageBoostDuration);
        Debug.Log($"[AdRewardManager] Damage boost started: {damageBoostMultiplier}x for {damageBoostDuration}s");
    }

    void EndDamageBoost()
    {
        isDamageBoostActive = false;
        damageBoostTimer = 0;

        OnDamageBoostEnded?.Invoke();
        Debug.Log("[AdRewardManager] Damage boost ended");
    }

    public void SetPendingOfflineReward(int earnings)
    {
        hasPendingOfflineReward = true;
        pendingOfflineEarnings = earnings;
        OnAdOffered?.Invoke(AdType.DoubleOffline);
    }

    public void RequestDoubleOffline(Action<bool> callback)
    {
        if (!hasPendingOfflineReward)
        {
            callback?.Invoke(false);
            return;
        }

        adService.ShowRewardedAd(
            AdType.DoubleOffline,
            () =>
            {
                lastAdTime[AdType.DoubleOffline] = Time.time;
                GrantDoubleOffline();
                callback?.Invoke(true);
            },
            () => callback?.Invoke(false)
        );
    }

    void GrantDoubleOffline()
    {
        int bonus = Mathf.RoundToInt(pendingOfflineEarnings * (offlineMultiplier - 1));
        currencyManager?.AddDuskenCoin(bonus);

        hasPendingOfflineReward = false;
        pendingOfflineEarnings = 0;

        OnRewardGranted?.Invoke(AdType.DoubleOffline, bonus);
        Debug.Log($"[AdRewardManager] Double offline earnings: +{bonus} Dusken Coin");
    }

    public bool IsDamageBoostActive => isDamageBoostActive;
    public float DamageBoostMultiplier => isDamageBoostActive ? damageBoostMultiplier : 1f;
    public float DamageBoostTimeRemaining => damageBoostTimer;
    public bool HasPendingOfflineReward => hasPendingOfflineReward;
    public int PendingOfflineEarnings => pendingOfflineEarnings;

    public float GetCooldownRemaining(AdType type)
    {
        float cooldown = GetCooldown(type);
        float elapsed = Time.time - lastAdTime[type];
        return Mathf.Max(0, cooldown - elapsed);
    }

    void OnDestroy()
    {
        if (hordeSpawner != null)
        {
            hordeSpawner.OnWaveCompleted -= OnWaveCompleted;
        }

        if (combatManager != null)
        {
            combatManager.OnThrallDied -= OnThrallDied;
        }
    }
}

