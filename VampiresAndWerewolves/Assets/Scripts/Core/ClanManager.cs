using UnityEngine;
using System;

public class ClanManager : MonoBehaviour
{
    public static ClanManager Instance { get; private set; }

    public event Action<ClanType> OnClanChanged;

    [SerializeField] private ClanType currentClan = ClanType.None;

    public ClanType CurrentClan => currentClan;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetClan(ClanType clan)
    {
        if (currentClan == clan) return;

        currentClan = clan;
        OnClanChanged?.Invoke(clan);
    }

    public ClanBonuses GetCurrentBonuses()
    {
        return GetBonusesForClan(currentClan);
    }

    public ClanBonuses GetBonusesForClan(ClanType clan)
    {
        switch (clan)
        {
            case ClanType.Nocturnum:
                return new ClanBonuses
                {
                    lifestealBonus = 0.05f,
                    attackSpeedBonus = 0f,
                    bleedChanceBonus = 0f
                };

            case ClanType.Sableheart:
                return new ClanBonuses
                {
                    lifestealBonus = 0f,
                    attackSpeedBonus = 0.1f,
                    bleedChanceBonus = 0f
                };

            case ClanType.Eclipsa:
                return new ClanBonuses
                {
                    lifestealBonus = 0f,
                    attackSpeedBonus = 0f,
                    bleedChanceBonus = 0.15f
                };

            default:
                return new ClanBonuses();
        }
    }

    public CombatStats ApplyBonusesToStats(CombatStats baseStats)
    {
        var bonuses = GetCurrentBonuses();

        return new CombatStats
        {
            maxHealth = baseStats.maxHealth,
            attack = baseStats.attack,
            defense = baseStats.defense,
            speed = baseStats.speed * (1f + bonuses.attackSpeedBonus),
            critChance = baseStats.critChance,
            lifestealPercent = baseStats.lifestealPercent + bonuses.lifestealBonus,
            bleedChance = baseStats.bleedChance + bonuses.bleedChanceBonus
        };
    }

    public string GetClanName(ClanType clan)
    {
        switch (clan)
        {
            case ClanType.Nocturnum:
                return "Clan Nocturnum";
            case ClanType.Sableheart:
                return "Clan Sableheart";
            case ClanType.Eclipsa:
                return "Clan Eclipsa";
            default:
                return "No Clan";
        }
    }

    public string GetClanDescription(ClanType clan)
    {
        switch (clan)
        {
            case ClanType.Nocturnum:
                return "Ancient royalty. Masters of blood rituals.\n+5% Lifesteal";
            case ClanType.Sableheart:
                return "Martial aristocrats. Known for brutality.\n+10% Attack Speed";
            case ClanType.Eclipsa:
                return "Arcane manipulators and shadowbinders.\n+15% Bleed Chance";
            default:
                return "Join a clan to receive powerful bonuses.";
        }
    }

    public Color GetClanColor(ClanType clan)
    {
        switch (clan)
        {
            case ClanType.Nocturnum:
                return new Color(0.6f, 0.1f, 0.1f);
            case ClanType.Sableheart:
                return new Color(0.2f, 0.2f, 0.3f);
            case ClanType.Eclipsa:
                return new Color(0.3f, 0.1f, 0.4f);
            default:
                return Color.gray;
        }
    }
}

[Serializable]
public struct ClanBonuses
{
    public float lifestealBonus;
    public float attackSpeedBonus;
    public float bleedChanceBonus;
}

