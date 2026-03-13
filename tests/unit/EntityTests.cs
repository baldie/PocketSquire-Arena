using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class EntityTests
    {
        [Test]
        public void TakeDamage_ReducesHealth()
        {
            // Arrange
            var entity = new Entity("Test", 10, 10, new Attributes());

            // Act
            entity.TakeDamage(4);

            // Assert
            Assert.That(entity.Health, Is.EqualTo(6));
            Assert.That(entity.IsDefeated, Is.False);
        }

        [Test]
        public void TakeDamage_CannotGoBelowZero()
        {
            // Arrange
            var entity = new Entity("Test", 10, 10, new Attributes());

            // Act
            entity.TakeDamage(15);

            // Assert
            Assert.That(entity.Health, Is.EqualTo(0));
            Assert.That(entity.IsDefeated, Is.True);
        }

        [Test]
        public void Heal_IncreasesHealth()
        {
            // Arrange
            var entity = new Entity("Test", 5, 10, new Attributes());

            // Act
            entity.Heal(3);

            // Assert
            Assert.That(entity.Health, Is.EqualTo(8));
        }

        [Test]
        public void Heal_CannotExceedMaxHealth()
        {
            // Arrange
            var entity = new Entity("Test", 8, 10, new Attributes());

            // Act
            entity.Heal(5);

            // Assert
            Assert.That(entity.Health, Is.EqualTo(10));
        }

        [Test]
        public void OnDeath_InvokedWhenHealthReachesZero()
        {
            // Arrange
            var entity = new Entity("Test", 10, 10, new Attributes());
            bool wasCalled = false;
            entity.onDeath += () => wasCalled = true;

            // Act
            entity.TakeDamage(10);

            // Assert
            Assert.That(wasCalled, Is.True);
        }
    [Test]
        public void TakeDamage_ReducesDamageWhenDefendingBasedOnDefense()
        {
            // Arrange
            var entity = new Entity("Test", 100, 100, new Attributes { Defense = 5 });
            entity.IsDefending = true;

            // Act
            entity.TakeDamage(10);

            // Assert
            Assert.That(entity.Health, Is.EqualTo(95), "Defense 5 should reduce a 10-damage hit to 5 after the ceiling step.");
        }

        [Test]
        public void TakeDamage_RoundingWhenDefending_RoundsUpWithScaledReduction()
        {
            // Arrange
            var entity = new Entity("Test", 100, 100, new Attributes { Defense = 20 });
            entity.IsDefending = true;

            // Act - 7 damage at 66% reduction leaves 2.38 damage, which ceilings to 3.
            entity.TakeDamage(7);

            // Assert
            Assert.That(entity.Health, Is.EqualTo(97), "Defending should still round up fractional damage after the scaled reduction.");
        }
    [Test]
        public void GetActionSoundId_ReturnsEmptyStringByDefault()
        {
            var entity = new Entity();
            // Default entity has no sound IDs set, so it returns empty
            Assert.That(entity.GetActionSoundId(ActionType.Attack), Is.EqualTo(string.Empty));
        }





        [Test]
        public void GetHitSoundId_ReturnsEmptyStringByDefault()
        {
            var entity = new Entity();
            Assert.That(entity.GetHitSoundId(), Is.EqualTo(string.Empty));
        }


        [Test]
        public void DefeatSoundId_CanBeSetAndRetrieved()
        {
            var entity = new Entity();
            entity.DefeatSoundId = "death_sound";
            Assert.That(entity.DefeatSoundId, Is.EqualTo("death_sound"));
        }
    }
}
