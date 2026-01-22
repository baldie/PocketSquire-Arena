using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class PlayerTests
    {
        [Test]
        public void Player_Initialization_SetsStatsCorrectly()
        {
            // Arrange
            var attributes = new Attributes();
            attributes.Strength = 1;
            attributes.Constitution = 1;
            attributes.Intelligence = 1;
            attributes.Wisdom = 1;
            attributes.Luck = 1;

            // Act
            var player = new Player("Squire", 10, 10, attributes, Player.CharGender.Male);

            // Assert
            Assert.That(player.Name, Is.EqualTo("Squire"));
            Assert.That(player.Health, Is.EqualTo(10));
            Assert.That(player.MaxHealth, Is.EqualTo(10));
            Assert.That(player.Attributes.Strength, Is.EqualTo(1));
            Assert.That(player.Attributes.Constitution, Is.EqualTo(1));
            Assert.That(player.Attributes.Intelligence, Is.EqualTo(1));
            Assert.That(player.Attributes.Wisdom, Is.EqualTo(1));
            Assert.That(player.Attributes.Luck, Is.EqualTo(1));
            Assert.That(player.IsDead, Is.False);
        }

        [Test]
        public void GainExperience_IncreasesExperience()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.CharGender.Male);
            
            // Act
            player.GainExperience(50);

            // Assert
            Assert.That(player.Experience, Is.EqualTo(50));
        }

        [Test]
        public void GainGold_IncreasesGold()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.CharGender.Male);
            
            // Act
            player.GainGold(100);

            // Assert
            Assert.That(player.Gold, Is.EqualTo(100));
        }
    }
}
