using NUnit.Framework;

[TestFixture]
public class CombatStatsTests
{
    private CombatStats CreateBaseStats()
    {
        return new CombatStats
        {
            maxHealth = 100f,
            attack = 20f,
            defense = 10f,
            speed = 1.0f,
            critChance = 0.1f,
            lifestealPercent = 0f,
            bleedChance = 0f
        };
    }

    [Test]
    public void Scale_MultipliesHealthAttackDefense()
    {
        var baseStats = CreateBaseStats();
        
        var scaled = CombatStats.Scale(baseStats, 2f);
        
        Assert.AreEqual(200f, scaled.maxHealth);
        Assert.AreEqual(40f, scaled.attack);
        Assert.AreEqual(20f, scaled.defense);
    }

    [Test]
    public void Scale_PreservesSpeed()
    {
        var baseStats = CreateBaseStats();
        baseStats.speed = 1.5f;
        
        var scaled = CombatStats.Scale(baseStats, 2f);
        
        Assert.AreEqual(1.5f, scaled.speed);
    }

    [Test]
    public void Scale_PreservesCritChance()
    {
        var baseStats = CreateBaseStats();
        baseStats.critChance = 0.15f;
        
        var scaled = CombatStats.Scale(baseStats, 3f);
        
        Assert.AreEqual(0.15f, scaled.critChance);
    }

    [Test]
    public void Scale_PreservesLifestealPercent()
    {
        var baseStats = CreateBaseStats();
        baseStats.lifestealPercent = 0.1f;
        
        var scaled = CombatStats.Scale(baseStats, 2f);
        
        Assert.AreEqual(0.1f, scaled.lifestealPercent);
    }

    [Test]
    public void Scale_PreservesBleedChance()
    {
        var baseStats = CreateBaseStats();
        baseStats.bleedChance = 0.05f;
        
        var scaled = CombatStats.Scale(baseStats, 2f);
        
        Assert.AreEqual(0.05f, scaled.bleedChance);
    }

    [Test]
    public void Scale_WithZeroMultiplier_ZeroesScaledStats()
    {
        var baseStats = CreateBaseStats();
        
        var scaled = CombatStats.Scale(baseStats, 0f);
        
        Assert.AreEqual(0f, scaled.maxHealth);
        Assert.AreEqual(0f, scaled.attack);
        Assert.AreEqual(0f, scaled.defense);
    }

    [Test]
    public void ApplyClanBonuses_Nocturnum_AddsLifesteal()
    {
        var stats = CreateBaseStats();
        stats.lifestealPercent = 0.05f;
        
        var result = CombatStats.ApplyClanBonuses(stats, ClanType.Nocturnum);
        
        Assert.AreEqual(0.10f, result.lifestealPercent, 0.001f);
    }

    [Test]
    public void ApplyClanBonuses_Sableheart_IncreasesSpeed()
    {
        var stats = CreateBaseStats();
        stats.speed = 1.0f;
        
        var result = CombatStats.ApplyClanBonuses(stats, ClanType.Sableheart);
        
        Assert.AreEqual(1.1f, result.speed, 0.001f);
    }

    [Test]
    public void ApplyClanBonuses_Eclipsa_AddsBleedChance()
    {
        var stats = CreateBaseStats();
        stats.bleedChance = 0.05f;
        
        var result = CombatStats.ApplyClanBonuses(stats, ClanType.Eclipsa);
        
        Assert.AreEqual(0.20f, result.bleedChance, 0.001f);
    }

    [Test]
    public void ApplyClanBonuses_None_ReturnsUnmodifiedStats()
    {
        var stats = CreateBaseStats();
        
        var result = CombatStats.ApplyClanBonuses(stats, ClanType.None);
        
        Assert.AreEqual(stats.maxHealth, result.maxHealth);
        Assert.AreEqual(stats.attack, result.attack);
        Assert.AreEqual(stats.defense, result.defense);
        Assert.AreEqual(stats.speed, result.speed);
        Assert.AreEqual(stats.lifestealPercent, result.lifestealPercent);
        Assert.AreEqual(stats.bleedChance, result.bleedChance);
    }

    [Test]
    public void ApplyClanBonuses_PreservesOtherStats()
    {
        var stats = CreateBaseStats();
        
        var result = CombatStats.ApplyClanBonuses(stats, ClanType.Nocturnum);
        
        Assert.AreEqual(stats.maxHealth, result.maxHealth);
        Assert.AreEqual(stats.attack, result.attack);
        Assert.AreEqual(stats.defense, result.defense);
        Assert.AreEqual(stats.critChance, result.critChance);
    }
}

