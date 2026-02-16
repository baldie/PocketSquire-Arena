using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class AttributesTests
    {
        [Test]
        public void Attributes_InitializesWithDefaultValues()
        {
            // Arrange & Act
            var attributes = new Attributes();

            // Assert
            Assert.That(attributes.Strength, Is.EqualTo(0));
            Assert.That(attributes.Constitution, Is.EqualTo(0));
            Assert.That(attributes.Magic, Is.EqualTo(0));
            Assert.That(attributes.Dexterity, Is.EqualTo(0));
            Assert.That(attributes.Luck, Is.EqualTo(0));
            Assert.That(attributes.Defense, Is.EqualTo(0));
        }

        [Test]
        public void Attributes_CanSetAllAttributes()
        {
            // Arrange
            var attributes = new Attributes();

            // Act
            attributes.Strength = 10;
            attributes.Constitution = 15;
            attributes.Magic = 12;
            attributes.Dexterity = 8;
            attributes.Luck = 5;
            attributes.Defense = 20;

            // Assert
            Assert.That(attributes.Strength, Is.EqualTo(10));
            Assert.That(attributes.Constitution, Is.EqualTo(15));
            Assert.That(attributes.Magic, Is.EqualTo(12));
            Assert.That(attributes.Dexterity, Is.EqualTo(8));
            Assert.That(attributes.Luck, Is.EqualTo(5));
            Assert.That(attributes.Defense, Is.EqualTo(20));
        }

        [Test]
        public void Player_InitializesWithDefenseAttribute()
        {
            // Arrange
            var attributes = new Attributes();
            attributes.Strength = 5;
            attributes.Constitution = 8;
            attributes.Magic = 6;
            attributes.Dexterity = 4;
            attributes.Luck = 3;
            attributes.Defense = 7;

            // Act
            var player = new Player("Defender", 10, 10, attributes, Player.CharGender.m);

            // Assert
            Assert.That(player.Attributes.Defense, Is.EqualTo(7));
        }

        [Test]
        public void Monster_InitializesWithDefenseAttribute()
        {
            // Arrange
            var attributes = new Attributes();
            attributes.Strength = 5;
            attributes.Constitution = 8;
            attributes.Magic = 6;
            attributes.Dexterity = 4;
            attributes.Luck = 3;
            attributes.Defense = 12;

            // Act
            var monster = new Monster("Dragon", 50, 50, attributes);

            // Assert
            Assert.That(monster.Attributes.Defense, Is.EqualTo(12));
        }

        [Test]
        public void Entity_InitializesWithDefenseAttribute()
        {
            // Arrange
            var attributes = new Attributes();
            attributes.Defense = 15;

            // Act
            var entity = new Entity("Test Entity", 100, 100, attributes);

            // Assert
            Assert.That(entity.Attributes.Defense, Is.EqualTo(15));
        }
    }
}
