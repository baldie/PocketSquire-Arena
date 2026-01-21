using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class PlayerTests
    {
        [Test]
        public void GetDefaultPlayer_ReturnsPlayerWithCorrectStats()
        {
            // Act
            var player = Player.GetDefaultPlayer();

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
            var player = Player.GetDefaultPlayer();
            
            // Act
            player.GainExperience(50);

            // Assert
            Assert.That(player.Experience, Is.EqualTo(50));
        }

        [Test]
        public void GainGold_IncreasesGold()
        {
            // Arrange
            var player = Player.GetDefaultPlayer();
            
            // Act
            player.GainGold(100);

            // Assert
            Assert.That(player.Gold, Is.EqualTo(100));
        }
    }
}
