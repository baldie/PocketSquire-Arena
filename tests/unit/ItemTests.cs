using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class ItemTests
    {
        [Test]
        public void Item_Initialization_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var item = new Item
            {
                Id = 1,
                Name = "Small Health Potion",
                Description = "Heals 25% of your health",
                Target = ItemTarget.Self,
                Stackable = true,
                Sprite = "health_potion",
                SoundEffect = "gulp",
                Price = 3
            };

            // Assert
            Assert.That(item.Id, Is.EqualTo(1));
            Assert.That(item.Name, Is.EqualTo("Small Health Potion"));
            Assert.That(item.Description, Is.EqualTo("Heals 25% of your health"));
            Assert.That(item.Target, Is.EqualTo(ItemTarget.Self));
            Assert.That(item.Stackable, Is.True);
            Assert.That(item.Sprite, Is.EqualTo("health_potion"));
            Assert.That(item.SoundEffect, Is.EqualTo("gulp"));
            Assert.That(item.Price, Is.EqualTo(3));
        }

        [Test]
        public void Item_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var item = new Item();

            // Assert
            Assert.That(item.Id, Is.EqualTo(0));
            Assert.That(item.Name, Is.EqualTo(string.Empty));
            Assert.That(item.Description, Is.EqualTo(string.Empty));
            Assert.That(item.Target, Is.EqualTo(ItemTarget.Self));
            Assert.That(item.Stackable, Is.True);
            Assert.That(item.Sprite, Is.EqualTo(string.Empty));
            Assert.That(item.SoundEffect, Is.EqualTo(string.Empty));
            Assert.That(item.Price, Is.EqualTo(0));
        }

        [Test]
        public void Item_TargetEnum_SupportsEnemyTarget()
        {
            // Arrange & Act
            var item = new Item
            {
                Id = 2,
                Name = "Throwing Knife",
                Target = ItemTarget.Enemy
            };

            // Assert
            Assert.That(item.Target, Is.EqualTo(ItemTarget.Enemy));
        }
    }
}
