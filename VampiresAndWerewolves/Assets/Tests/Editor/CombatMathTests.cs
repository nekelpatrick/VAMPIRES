using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class CombatMathTests
{
    [Test]
    public void ComputeDamage_WithHigherAttackThanDefense_ReturnsPositiveDamage()
    {
        int damage = CombatMath.ComputeDamage(50f, 20f);
        
        Assert.AreEqual(40, damage);
    }

    [Test]
    public void ComputeDamage_AppliesDefenseMitigation()
    {
        int damage = CombatMath.ComputeDamage(100f, 40f);
        
        Assert.AreEqual(80, damage);
    }

    [Test]
    public void ComputeDamage_MinimumDamageIsOne()
    {
        int damage = CombatMath.ComputeDamage(10f, 100f);
        
        Assert.AreEqual(1, damage);
    }

    [Test]
    public void ComputeDamage_ZeroDefense_ReturnsFullAttack()
    {
        int damage = CombatMath.ComputeDamage(25f, 0f);
        
        Assert.AreEqual(25, damage);
    }

    [Test]
    public void ComputeDamage_FloorsResult()
    {
        int damage = CombatMath.ComputeDamage(25f, 5f);
        
        Assert.AreEqual(22, damage);
    }

    [Test]
    public void ApplyVariance_WithZeroRoll_DecreasesBaseDamage()
    {
        int result = CombatMath.ApplyVariance(100, 0.2f, 0f);
        
        Assert.AreEqual(90, result);
    }

    [Test]
    public void ApplyVariance_WithHalfRoll_ReturnsBaseDamage()
    {
        int result = CombatMath.ApplyVariance(100, 0.2f, 0.5f);
        
        Assert.AreEqual(100, result);
    }

    [Test]
    public void ApplyVariance_WithFullRoll_IncreasesBaseDamage()
    {
        int result = CombatMath.ApplyVariance(100, 0.2f, 1f);
        
        Assert.AreEqual(110, result);
    }

    [Test]
    public void ApplyVariance_MinimumResultIsOne()
    {
        int result = CombatMath.ApplyVariance(1, 0.5f, 0f);
        
        Assert.GreaterOrEqual(result, 1);
    }

    [Test]
    public void ApplyVariance_ZeroVariance_ReturnsBaseDamage()
    {
        int result = CombatMath.ApplyVariance(50, 0f, 0.75f);
        
        Assert.AreEqual(50, result);
    }
}


