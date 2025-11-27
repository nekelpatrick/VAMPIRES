using System;
using UnityEngine;

public enum AdType
{
    DoubleLoot,
    InstantRevive,
    DamageBoost,
    DoubleOffline
}

public interface IAdService
{
    bool IsAdReady(AdType type);
    void ShowRewardedAd(AdType type, Action onRewardGranted, Action onAdFailed);
    void LoadAd(AdType type);
}

public class AdService : MonoBehaviour, IAdService
{
    public static AdService Instance { get; private set; }

    public event Action<AdType> OnAdCompleted;
    public event Action<AdType> OnAdFailed;
    public event Action<AdType> OnAdStarted;

    private bool simulateAdSuccess = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAds()
    {
        Debug.Log("[AdService] Initialized - Using placeholder implementation");
        Debug.Log("[AdService] TODO: Replace with Unity Ads SDK when ready for production");
    }

    public bool IsAdReady(AdType type)
    {
        return true;
    }

    public void LoadAd(AdType type)
    {
        Debug.Log($"[AdService] Loading ad for {type}");
    }

    public void ShowRewardedAd(AdType type, Action onRewardGranted, Action onAdFailed)
    {
        Debug.Log($"[AdService] Showing rewarded ad for {type}");
        OnAdStarted?.Invoke(type);

        if (simulateAdSuccess)
        {
            SimulateAdWatched(type, onRewardGranted);
        }
        else
        {
            SimulateAdFailed(type, onAdFailed);
        }
    }

    void SimulateAdWatched(AdType type, Action onRewardGranted)
    {
        Debug.Log($"[AdService] Ad completed successfully for {type}");
        onRewardGranted?.Invoke();
        OnAdCompleted?.Invoke(type);
    }

    void SimulateAdFailed(AdType type, Action onAdFailed)
    {
        Debug.Log($"[AdService] Ad failed for {type}");
        onAdFailed?.Invoke();
        OnAdFailed?.Invoke(type);
    }

    public void SetSimulateSuccess(bool success)
    {
        simulateAdSuccess = success;
    }
}

