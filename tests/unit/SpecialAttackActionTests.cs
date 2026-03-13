using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class SpecialAttackActionTests
    {
        [Test]
        public void SpecialAttackAction_Type_IsSpecialAttack()
        {
            var attacker = new Monster("Test Monster", 100, 100, new Attributes { Strength = 5 });
            var target = new Player { Name = "Test Player", Health = 100, MaxHealth = 100 };

            var action = new SpecialAttackAction(attacker, target);

            Assert.That(action.Type, Is.EqualTo(ActionType.SpecialAttack));
        }

        [Test]
        public void SpecialAttackAction_WhenHit_Damage_UsesStrength()
        {
            var attacker = new Monster("Test Monster", 100, 100, new Attributes { Strength = 7, Dexterity = 100 }); // 99% hit
            var target = new Player { Name = "Test Player", Health = 100, MaxHealth = 100 };

            // Find a seed that guarantees a normal hit (no crit) with 99% hit chance
            for (int seed = 0; seed < 1000; seed++)
            {
                var rng = new System.Random(seed);
                var action = new SpecialAttackAction(attacker, target, rng);
                if (action.DidHit && !action.IsCrit)
                {
                    Assert.That(action.Damage, Is.EqualTo(33));
                    return;
                }
            }
            Assert.Fail("Could not find a normal hit with seed search");
        }

        [Test]
        public void SpecialAttackAction_Damage_MinimumIsOne_WhenHit()
        {
            var attacker = new Monster("Weak Monster", 100, 100, new Attributes { Strength = 0, Dexterity = 100 }); // 99% hit
            var target = new Player { Name = "Test Player", Health = 100, MaxHealth = 100 };

            // All hits should deal at least 1 damage (Math.Max(1, Strength))
            for (int seed = 0; seed < 50; seed++)
            {
                var rng = new System.Random(seed);
                var action = new SpecialAttackAction(attacker, target, rng);
                if (action.DidHit)
                {
                    Assert.That(action.Damage, Is.GreaterThanOrEqualTo(1));
                    return;
                }
            }
            Assert.Fail("Could not find a hit");
        }

        [Test]
        public void SpecialAttackAction_ApplyEffect_DealsDamageToTarget_WhenHit()
        {
            var attacker = new Monster("Test Monster", 100, 100, new Attributes { Strength = 10, Dexterity = 100 });
            var target = new Player { Name = "Test Player", Health = 100, MaxHealth = 100 };

            // Find a normal hit
            for (int seed = 0; seed < 1000; seed++)
            {
                var rng = new System.Random(seed);
                var action = new SpecialAttackAction(attacker, target, rng);
                if (action.DidHit && !action.IsCrit)
                {
                    target.Health = 100;
                    action.ApplyEffect();
                    Assert.That(target.Health, Is.EqualTo(100 - action.FinalDamage), $"Expected HP to reflect the resolved final damage with seed {seed}");
                    return;
                }
            }
            Assert.Fail("Could not find a non-crit hit");
        }

        [Test]
        public void SpecialAttackAction_ApplyEffect_NoHPChange_OnMiss()
        {
            var attacker = new Monster("Test Monster", 100, 100, new Attributes { Strength = 10, Dexterity = 0 });
            var target = new Player { Name = "Test Player", Health = 100, MaxHealth = 100 };

            for (int seed = 0; seed < 1000; seed++)
            {
                var rng = new System.Random(seed);
                var action = new SpecialAttackAction(attacker, target, rng);
                if (!action.DidHit)
                {
                    int before = target.Health;
                    action.ApplyEffect();
                    Assert.That(target.Health, Is.EqualTo(before), "Miss should not change target HP");
                    return;
                }
            }
            Assert.Ignore("Could not find miss; skip on edge case");
        }

        [Test]
        public void SpecialAttackAction_Actor_IsCorrectlySet()
        {
            var attacker = new Monster("Attacker", 100, 100, new Attributes());
            var target = new Player { Name = "Target" };

            var action = new SpecialAttackAction(attacker, target);

            Assert.That(action.Actor, Is.SameAs(attacker));
        }

        [Test]
        public void SpecialAttackAction_Target_IsCorrectlySet()
        {
            var attacker = new Monster("Attacker", 100, 100, new Attributes());
            var target = new Player { Name = "Target" };

            var action = new SpecialAttackAction(attacker, target);

            Assert.That(action.Target, Is.SameAs(target));
        }

        [Test]
        public void SpecialAttackAction_Constructor_ThrowsOnNullActor()
        {
            var target = new Player { Name = "Target" };

            Assert.Throws<System.ArgumentNullException>(() => new SpecialAttackAction(null!, target));
        }

        [Test]
        public void SpecialAttackAction_Constructor_ThrowsOnNullTarget()
        {
            var attacker = new Monster("Attacker", 100, 100, new Attributes());

            Assert.Throws<System.ArgumentNullException>(() => new SpecialAttackAction(attacker, null!));
        }
    }
}
