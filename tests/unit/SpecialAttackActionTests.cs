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
        public void SpecialAttackAction_Damage_UsesStrength()
        {
            var attacker = new Monster("Test Monster", 100, 100, new Attributes { Strength = 7 });
            var target = new Player { Name = "Test Player", Health = 100, MaxHealth = 100 };

            var action = new SpecialAttackAction(attacker, target);

            // Damage should be based on Strength (7)
            Assert.That(action.Damage, Is.EqualTo(7));
        }

        [Test]
        public void SpecialAttackAction_Damage_MinimumIsOne()
        {
            var attacker = new Monster("Weak Monster", 100, 100, new Attributes { Strength = 0 });
            var target = new Player { Name = "Test Player", Health = 100, MaxHealth = 100 };

            var action = new SpecialAttackAction(attacker, target);

            Assert.That(action.Damage, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void SpecialAttackAction_ApplyEffect_DealsDamageToTarget()
        {
            var attacker = new Monster("Test Monster", 100, 100, new Attributes { Strength = 10 });
            var target = new Player { Name = "Test Player", Health = 100, MaxHealth = 100 };

            var action = new SpecialAttackAction(attacker, target);
            action.ApplyEffect();

            Assert.That(target.Health, Is.EqualTo(90));
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
