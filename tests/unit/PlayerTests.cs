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
            attributes.Magic = 1;
            attributes.Dexterity = 1;
            attributes.Luck = 1;
            attributes.Defense = 1;

            // Act
            var player = new Player("Squire", 10, 10, attributes, Player.Genders.m);

            // Assert
            Assert.That(player.Name, Is.EqualTo("Squire"));
            Assert.That(player.Health, Is.EqualTo(10));
            Assert.That(player.MaxHealth, Is.EqualTo(10));
            Assert.That(player.Attributes.Strength, Is.EqualTo(1));
            Assert.That(player.Attributes.Constitution, Is.EqualTo(1));
            Assert.That(player.Attributes.Magic, Is.EqualTo(1));
            Assert.That(player.Attributes.Dexterity, Is.EqualTo(1));
            Assert.That(player.Attributes.Luck, Is.EqualTo(1));
            Assert.That(player.Attributes.Defense, Is.EqualTo(1));
            Assert.That(player.IsDefeated, Is.False);
        }

        [Test]
        public void GainExperience_IncreasesExperience()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            
            // Act
            player.GainExperience(50);

            // Assert
            Assert.That(player.Experience, Is.EqualTo(50));
        }

        [Test]
        public void GainGold_IncreasesGold()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            
            // Act
            player.GainGold(100);

            // Assert
            Assert.That(player.Gold, Is.EqualTo(100));
        }

        [Test]
        public void Player_SpriteIds_ReturnCorrectFormattedStrings()
        {
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            player.Experience = 0; // Level 1 is calculated as (Experience+1)/100 rounded up. 0 exp -> Level 1.

            Assert.That(player.AttackSpriteId, Is.EqualTo("m_squire_attack"));
            Assert.That(player.DefendSpriteId, Is.EqualTo("m_squire_defend"));
            Assert.That(player.HitSpriteId, Is.EqualTo("m_squire_hit"));
            Assert.That(player.DefeatSpriteId, Is.EqualTo("m_squire_defeat"));
            Assert.That(player.WinSpriteId, Is.EqualTo("m_squire_win"));
            Assert.That(player.BattleSpriteId, Is.EqualTo("m_squire_battle"));
        }

        [Test]
        public void Player_HasEmptyInventory_ByDefault()
        {
            // Arrange & Act
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);

            // Assert
            Assert.That(player.Inventory, Is.Not.Null);
            Assert.That(player.Inventory.Slots.Count, Is.EqualTo(0));
        }

        [Test]
        public void Player_Inventory_PersistsItemsCorrectly()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            
            // Act — base capacity is 2 slots / stack 2, so 2 different items fit fine
            player.Inventory.AddItem(1, 2);
            player.Inventory.AddItem(2, 1);

            // Assert
            Assert.That(player.Inventory.GetItemCount(1), Is.EqualTo(2));
            Assert.That(player.Inventory.GetItemCount(2), Is.EqualTo(1));
            Assert.That(player.Inventory.Slots.Count, Is.EqualTo(2));
        }

        [Test]
        public void SpendGold_ReducesGold()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            player.GainGold(100);
            
            // Act
            player.SpendGold(30);

            // Assert
            Assert.That(player.Gold, Is.EqualTo(70));
        }

        [Test]
        public void SpendGold_Throws_WhenInsufficientGold()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            player.GainGold(50);
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => player.SpendGold(100));
            Assert.That(player.Gold, Is.EqualTo(50), "Gold should not change when exception is thrown");
        }

        [Test]
        public void TryPurchaseItem_Succeeds_WithEnoughGold()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            player.GainGold(100);
            var item = new Item { Id = 5, Name = "Sword", Price = 50 };
            
            // Act
            var result = player.TryPurchaseItem(item);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(player.Gold, Is.EqualTo(50));
        }

        [Test]
        public void TryPurchaseItem_Fails_WithInsufficientGold()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            player.GainGold(30);
            var item = new Item { Id = 5, Name = "Sword", Price = 50 };
            
            // Act
            var result = player.TryPurchaseItem(item);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(player.Gold, Is.EqualTo(30), "Gold should not change on failed purchase");
            Assert.That(player.Inventory.GetItemCount(5), Is.EqualTo(0), "Item should not be added on failed purchase");
        }

        [Test]
        public void TryPurchaseItem_AddsItemToInventory()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            player.GainGold(100);
            var item = new Item { Id = 7, Name = "Potion", Price = 25 };
            
            // Act
            var result = player.TryPurchaseItem(item);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(player.Inventory.GetItemCount(7), Is.EqualTo(1));
            Assert.That(player.Inventory.Slots.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryPurchaseItem_Fails_WhenInventoryFull()
        {
            // Arrange — fill both base slots to their stack limit
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            player.GainGold(500);
            player.Inventory.AddItem(1, 2); // slot 1 full
            player.Inventory.AddItem(2, 2); // slot 2 full
            var item = new Item { Id = 3, Name = "Elixir", Price = 10 };

            // Act
            var result = player.TryPurchaseItem(item);

            // Assert
            Assert.That(result, Is.False, "Purchase must be rejected when inventory is full");
            Assert.That(player.Gold, Is.EqualTo(500), "Gold must not be spent");
            Assert.That(player.Inventory.GetItemCount(3), Is.EqualTo(0));
        }

        [Test]
        public void TryPurchasePerk_Succeeds_WithEnoughGold()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            player.GainGold(200);
            var perk = new PocketSquire.Arena.Core.LevelUp.Perk("satchel_tier1", "Satchel", "Bigger bag", 1, null,
                new System.Collections.Generic.List<PlayerClass.ClassName>(), price: 100);

            // Act
            var result = player.TryPurchasePerk(perk);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(player.Gold, Is.EqualTo(100));
            Assert.That(player.UnlockedPerks.Contains("satchel_tier1"), Is.True);
        }

        [Test]
        public void TryPurchasePerk_Fails_WithInsufficientGold()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            player.GainGold(50);
            var perk = new PocketSquire.Arena.Core.LevelUp.Perk("satchel_tier1", "Satchel", "Bigger bag", 1, null,
                new System.Collections.Generic.List<PlayerClass.ClassName>(), price: 100);

            // Act
            var result = player.TryPurchasePerk(perk);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(player.Gold, Is.EqualTo(50), "Gold should not change on failed purchase");
            Assert.That(player.UnlockedPerks.Contains("satchel_tier1"), Is.False);
        }

        [Test]
        public void TryPurchasePerk_Fails_WhenAlreadyOwned()
        {
            // Arrange
            var player = new Player("Squire", 10, 10, new Attributes(), Player.Genders.m);
            player.GainGold(500);
            var perk = new PocketSquire.Arena.Core.LevelUp.Perk("satchel_tier1", "Satchel", "Bigger bag", 1, null,
                new System.Collections.Generic.List<PlayerClass.ClassName>(), price: 100);

            // Buy it once
            player.TryPurchasePerk(perk);

            // Act — attempt to buy again
            var result = player.TryPurchasePerk(perk);

            // Assert
            Assert.That(result, Is.False, "Cannot purchase the same perk twice");
            Assert.That(player.Gold, Is.EqualTo(400), "Gold should only be spent once");
        }

    }
}
