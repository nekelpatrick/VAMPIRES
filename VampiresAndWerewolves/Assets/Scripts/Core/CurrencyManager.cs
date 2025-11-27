using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public event Action<int, int> OnCurrencyChanged;

    public int DuskenCoin { get; private set; }
    public int BloodShards { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddDuskenCoin(int amount)
    {
        DuskenCoin += amount;
        OnCurrencyChanged?.Invoke(DuskenCoin, BloodShards);
    }

    public void AddBloodShards(int amount)
    {
        BloodShards += amount;
        OnCurrencyChanged?.Invoke(DuskenCoin, BloodShards);
    }

    public bool SpendDuskenCoin(int amount)
    {
        if (DuskenCoin < amount) return false;
        DuskenCoin -= amount;
        OnCurrencyChanged?.Invoke(DuskenCoin, BloodShards);
        return true;
    }

    public bool SpendBloodShards(int amount)
    {
        if (BloodShards < amount) return false;
        BloodShards -= amount;
        OnCurrencyChanged?.Invoke(DuskenCoin, BloodShards);
        return true;
    }
}

