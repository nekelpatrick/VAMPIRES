using NUnit.Framework;

[TestFixture]
public class RewardCalculatorTests
{
    [Test]
    public void CalculateReward_Wave1_ReturnsBaseDusken()
    {
        var reward = RewardCalculator.CalculateReward(1, false, 0.5f);
        
        Assert.AreEqual(10, reward.duskenCoin);
    }

    [Test]
    public void CalculateReward_HigherWave_IncreasesReward()
    {
        var wave1 = RewardCalculator.CalculateReward(1, false, 0.5f);
        var wave10 = RewardCalculator.CalculateReward(10, false, 0.5f);
        
        Assert.Greater(wave10.duskenCoin, wave1.duskenCoin);
    }

    [Test]
    public void CalculateReward_Elite_DoublesDusken()
    {
        var normal = RewardCalculator.CalculateReward(5, false, 0.5f);
        var elite = RewardCalculator.CalculateReward(5, true, 0.5f);
        
        Assert.AreEqual(normal.duskenCoin * 2, elite.duskenCoin);
    }

    [Test]
    public void CalculateReward_NormalEnemy_LowBloodShardChance()
    {
        var reward = RewardCalculator.CalculateReward(1, false, 0.06f);
        
        Assert.AreEqual(0, reward.bloodShards);
    }

    [Test]
    public void CalculateReward_NormalEnemy_CanDropBloodShard()
    {
        var reward = RewardCalculator.CalculateReward(1, false, 0.01f);
        
        Assert.AreEqual(1, reward.bloodShards);
    }

    [Test]
    public void CalculateReward_Elite_HigherBloodShardChance()
    {
        var reward = RewardCalculator.CalculateReward(1, true, 0.1f);
        
        Assert.AreEqual(1, reward.bloodShards);
    }

    [Test]
    public void CalculateReward_Elite_NoBloodShardOnHighRoll()
    {
        var reward = RewardCalculator.CalculateReward(1, true, 0.5f);
        
        Assert.AreEqual(0, reward.bloodShards);
    }

    [Test]
    public void CalculateReward_WaveScaling_CorrectFormula()
    {
        var reward = RewardCalculator.CalculateReward(5, false, 0.5f);
        
        Assert.AreEqual(8 + 5 * 2, reward.duskenCoin);
    }

    [Test]
    public void WaveCompletionBonus_Wave10_ReturnsOne()
    {
        int bonus = RewardCalculator.WaveCompletionBonus(10);
        
        Assert.AreEqual(1, bonus);
    }

    [Test]
    public void WaveCompletionBonus_Wave20_ReturnsOne()
    {
        int bonus = RewardCalculator.WaveCompletionBonus(20);
        
        Assert.AreEqual(1, bonus);
    }

    [Test]
    public void WaveCompletionBonus_NonMultipleOf10_ReturnsZero()
    {
        int bonus5 = RewardCalculator.WaveCompletionBonus(5);
        int bonus15 = RewardCalculator.WaveCompletionBonus(15);
        int bonus99 = RewardCalculator.WaveCompletionBonus(99);
        
        Assert.AreEqual(0, bonus5);
        Assert.AreEqual(0, bonus15);
        Assert.AreEqual(0, bonus99);
    }

    [Test]
    public void WaveCompletionBonus_Wave100_ReturnsOne()
    {
        int bonus = RewardCalculator.WaveCompletionBonus(100);
        
        Assert.AreEqual(1, bonus);
    }
}

