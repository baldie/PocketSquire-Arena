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
            Assert.That(entity.IsDead, Is.False);
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
            Assert.That(entity.IsDead, Is.True);
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
        public void TakeDamage_ReducesDamageWhenDefending()
        {
            // Arrange
            var entity = new Entity("Test", 100, 100, new Attributes());
            entity.IsDefending = true;

            // Act
            entity.TakeDamage(10);

            // Assert
            Assert.That(entity.Health, Is.EqualTo(95), "Damage should be reduced by 50%");
        }

        [Test]
        public void TakeDamage_RoundingWhenDefending_RoundsUp()
        {
            // Arrange
            var entity = new Entity("Test", 100, 100, new Attributes());
            entity.IsDefending = true;

            // Act - 5 / 2 = 2.5 -> Ceil(2.5) should be 3 damage taken
            entity.TakeDamage(5);

            // Assert
            Assert.That(entity.Health, Is.EqualTo(97), "Defending damage of 5 should result in 3 damage taken (2.5 rounded up)");
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


    }
}
