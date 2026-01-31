using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class InventoryTests
    {
        [Test]
        public void AddItem_AddsNewSlot_WhenItemNotPresent()
        {
            // Arrange
            var inventory = new Inventory();

            // Act
            inventory.AddItem(1, 1);

            // Assert
            Assert.That(inventory.GetItemCount(1), Is.EqualTo(1));
            Assert.That(inventory.Slots.Count, Is.EqualTo(1));
            Assert.That(inventory.Slots[0].ItemId, Is.EqualTo(1));
            Assert.That(inventory.Slots[0].Quantity, Is.EqualTo(1));
        }

        [Test]
        public void AddItem_StacksQuantity_WhenItemExists()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.AddItem(1, 2);

            // Act
            inventory.AddItem(1, 3);

            // Assert
            Assert.That(inventory.GetItemCount(1), Is.EqualTo(5));
            Assert.That(inventory.Slots.Count, Is.EqualTo(1), "Should not create duplicate slots");
        }

        [Test]
        public void AddItem_AddsMultipleQuantity()
        {
            // Arrange
            var inventory = new Inventory();

            // Act
            inventory.AddItem(1, 5);

            // Assert
            Assert.That(inventory.GetItemCount(1), Is.EqualTo(5));
        }

        [Test]
        public void RemoveItem_DecreasesQuantity()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.AddItem(1, 5);

            // Act
            var result = inventory.RemoveItem(1, 2);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(inventory.GetItemCount(1), Is.EqualTo(3));
        }

        [Test]
        public void RemoveItem_RemovesSlot_WhenQuantityReachesZero()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.AddItem(1, 3);

            // Act
            var result = inventory.RemoveItem(1, 3);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(inventory.GetItemCount(1), Is.EqualTo(0));
            Assert.That(inventory.Slots.Count, Is.EqualTo(0), "Slot should be removed when quantity reaches zero");
        }

        [Test]
        public void RemoveItem_ReturnsFalse_WhenItemNotPresent()
        {
            // Arrange
            var inventory = new Inventory();

            // Act
            var result = inventory.RemoveItem(1, 1);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void RemoveItem_ReturnsFalse_WhenInsufficientQuantity()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.AddItem(1, 2);

            // Act
            var result = inventory.RemoveItem(1, 5);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(inventory.GetItemCount(1), Is.EqualTo(2), "Quantity should not change on failed removal");
        }

        [Test]
        public void GetItemCount_ReturnsCorrectQuantity()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.AddItem(1, 7);

            // Act
            var count = inventory.GetItemCount(1);

            // Assert
            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void GetItemCount_ReturnsZero_WhenItemNotPresent()
        {
            // Arrange
            var inventory = new Inventory();

            // Act
            var count = inventory.GetItemCount(99);

            // Assert
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void HasItem_ReturnsTrue_WhenItemPresent()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.AddItem(1, 1);

            // Act
            var hasItem = inventory.HasItem(1);

            // Assert
            Assert.That(hasItem, Is.True);
        }

        [Test]
        public void HasItem_ReturnsFalse_WhenItemNotPresent()
        {
            // Arrange
            var inventory = new Inventory();

            // Act
            var hasItem = inventory.HasItem(1);

            // Assert
            Assert.That(hasItem, Is.False);
        }

        [Test]
        public void Inventory_SupportsMultipleItemTypes()
        {
            // Arrange
            var inventory = new Inventory();

            // Act
            inventory.AddItem(1, 3); // Health potions
            inventory.AddItem(2, 5); // Mana potions
            inventory.AddItem(3, 1); // Rare item

            // Assert
            Assert.That(inventory.Slots.Count, Is.EqualTo(3));
            Assert.That(inventory.GetItemCount(1), Is.EqualTo(3));
            Assert.That(inventory.GetItemCount(2), Is.EqualTo(5));
            Assert.That(inventory.GetItemCount(3), Is.EqualTo(1));
        }
    }
}
