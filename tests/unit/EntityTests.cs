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
    }
}
