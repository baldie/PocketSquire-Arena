using NUnit.Framework;
using PocketSquire.Arena.Core;
using System.Collections.Generic;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class InventoryTests
    {
        // ─── InventorySlot POCO ───────────────────────────────────────────────

        [Test]
        public void InventorySlot_IsFull_ReturnsFalse_WhenBelowLimit()
        {
            var slot = new InventorySlot { ItemId = 1, Quantity = 1 };
            Assert.That(slot.IsFull(2), Is.False);
        }

        [Test]
        public void InventorySlot_IsFull_ReturnsTrue_WhenAtLimit()
        {
            var slot = new InventorySlot { ItemId = 1, Quantity = 2 };
            Assert.That(slot.IsFull(2), Is.True);
        }

        [Test]
        public void InventorySlot_CanStack_ReturnsTrue_ForMatchingItem()
        {
            var slot = new InventorySlot { ItemId = 5, Quantity = 1 };
            Assert.That(slot.CanStack(5), Is.True);
        }

        [Test]
        public void InventorySlot_CanStack_ReturnsFalse_ForDifferentItem()
        {
            var slot = new InventorySlot { ItemId = 5, Quantity = 1 };
            Assert.That(slot.CanStack(9), Is.False);
        }

        // ─── UpdateCapacity ───────────────────────────────────────────────────

        [Test]
        public void UpdateCapacity_BaseValues_WithNoPerks()
        {
            var inv = new Inventory();
            inv.UpdateCapacity(new HashSet<string>());
            Assert.That(inv.MaxSlots,     Is.EqualTo(2));
            Assert.That(inv.MaxStackSize, Is.EqualTo(2));
        }

        [Test]
        public void UpdateCapacity_Tier1_Expands()
        {
            var inv = new Inventory();
            inv.UpdateCapacity(new HashSet<string> { "satchel_tier_1" });
            Assert.That(inv.MaxSlots,     Is.EqualTo(3));
            Assert.That(inv.MaxStackSize, Is.EqualTo(3));
        }

        [Test]
        public void UpdateCapacity_Tier2_Expands()
        {
            var inv = new Inventory();
            inv.UpdateCapacity(new HashSet<string> { "satchel_tier_2" });
            Assert.That(inv.MaxSlots,     Is.EqualTo(4));
            Assert.That(inv.MaxStackSize, Is.EqualTo(4));
        }

        [Test]
        public void UpdateCapacity_Tier3_Expands()
        {
            var inv = new Inventory();
            inv.UpdateCapacity(new HashSet<string> { "satchel_tier_3" });
            Assert.That(inv.MaxSlots,     Is.EqualTo(5));
            Assert.That(inv.MaxStackSize, Is.EqualTo(5));
        }

        [Test]
        public void UpdateCapacity_HigherTierOverridesLower()
        {
            // Has tier 1 and tier 3 — should use tier 3
            var inv = new Inventory();
            inv.UpdateCapacity(new HashSet<string> { "satchel_tier_1", "satchel_tier_3" });
            Assert.That(inv.MaxSlots,     Is.EqualTo(5));
            Assert.That(inv.MaxStackSize, Is.EqualTo(5));
        }

        [Test]
        public void UpdateCapacity_DoesNotLoseExistingItems()
        {
            // Fill 3 slots at tier 1, then downgrade to base — items are kept
            var inv = new Inventory();
            inv.UpdateCapacity(new HashSet<string> { "satchel_tier_1" });
            inv.AddItem(1, 1);
            inv.AddItem(2, 1);
            inv.AddItem(3, 1);
            Assert.That(inv.Slots.Count, Is.EqualTo(3));

            inv.UpdateCapacity(new HashSet<string>()); // back to base (2 slots)
            // Existing items are preserved even though MaxSlots is now 2
            Assert.That(inv.Slots.Count, Is.EqualTo(3));
        }

        // ─── HasRoom ─────────────────────────────────────────────────────────

        [Test]
        public void HasRoom_ReturnsTrue_WhenSlotCanStack()
        {
            var inv = new Inventory(); // base: 2 slots, stack 2
            inv.AddItem(1, 1);         // slot 1: qty 1 (room for 1 more)
            Assert.That(inv.HasRoom(1), Is.True);
        }

        [Test]
        public void HasRoom_ReturnsTrue_WhenEmptySlotAvailable()
        {
            var inv = new Inventory(); // base: 2 slots
            inv.AddItem(1, 2);         // slot 1 full
            Assert.That(inv.HasRoom(2), Is.True); // slot 2 still free
        }

        [Test]
        public void HasRoom_ReturnsFalse_WhenAllSlotsFullAndAtMaxStack()
        {
            var inv = new Inventory(); // 2 slots, stack 2
            inv.AddItem(1, 2);         // slot 1 full
            inv.AddItem(2, 2);         // slot 2 full
            Assert.That(inv.HasRoom(3), Is.False);
        }

        // ─── AddItem ─────────────────────────────────────────────────────────

        [Test]
        public void AddItem_AddsNewSlot_WhenItemNotPresent()
        {
            var inv = new Inventory();
            var result = inv.AddItem(1, 1);
            Assert.That(result, Is.True);
            Assert.That(inv.GetItemCount(1), Is.EqualTo(1));
            Assert.That(inv.Slots.Count,     Is.EqualTo(1));
        }

        [Test]
        public void AddItem_StacksOntoExistingSlot_BeforeOpeningNewSlot()
        {
            var inv = new Inventory(); // 2 slots, stack 2
            inv.AddItem(1, 1);
            var result = inv.AddItem(1, 1);
            Assert.That(result, Is.True);
            Assert.That(inv.Slots.Count,     Is.EqualTo(1), "Should not open a second slot");
            Assert.That(inv.GetItemCount(1), Is.EqualTo(2));
        }

        [Test]
        public void AddItem_ReturnsFalse_WhenStackLimitReached()
        {
            var inv = new Inventory(); // stack 2
            inv.AddItem(1, 2);         // slot full
            var result = inv.AddItem(1, 1);
            Assert.That(result, Is.False);
            Assert.That(inv.GetItemCount(1), Is.EqualTo(2), "Quantity must not change");
        }

        [Test]
        public void AddItem_ReturnsFalse_WhenAllSlotsFull()
        {
            var inv = new Inventory(); // 2 slots, stack 2
            inv.AddItem(1, 2);         // slot 1 full
            inv.AddItem(2, 2);         // slot 2 full
            var result = inv.AddItem(3, 1);
            Assert.That(result, Is.False);
            Assert.That(inv.Slots.Count, Is.EqualTo(2));
        }

        [Test]
        public void AddItem_SupportsMultipleItemTypes_WithExpandedCapacity()
        {
            var inv = new Inventory();
            inv.UpdateCapacity(new HashSet<string> { "satchel_tier_1" }); // 3 slots, stack 3
            inv.AddItem(1, 3);
            inv.AddItem(2, 3);
            inv.AddItem(3, 1);
            Assert.That(inv.Slots.Count,     Is.EqualTo(3));
            Assert.That(inv.GetItemCount(1), Is.EqualTo(3));
            Assert.That(inv.GetItemCount(2), Is.EqualTo(3));
            Assert.That(inv.GetItemCount(3), Is.EqualTo(1));
        }

        [Test]
        public void AddItem_PartialAdd_WhenSomeUnitsFit()
        {
            // 1 slot free, stack 2. Add qty 3 — first 2 fit (fill slot), 3rd rejected.
            var inv = new Inventory(); // 2 slots, stack 2
            inv.AddItem(2, 2);         // fill slot 1
            // slot 2 is free; item 1 will fill it to stack 2 then fail on 3rd unit
            var result = inv.AddItem(1, 3);
            Assert.That(result, Is.False);
            Assert.That(inv.GetItemCount(1), Is.EqualTo(2)); // 2 units did fit
        }

        // ─── RemoveItem ───────────────────────────────────────────────────────

        [Test]
        public void RemoveItem_DecreasesQuantity()
        {
            var inv = new Inventory();
            inv.UpdateCapacity(new HashSet<string> { "satchel_tier_1" }); // stack 3
            inv.AddItem(1, 3);
            var result = inv.RemoveItem(1, 2);
            Assert.That(result, Is.True);
            Assert.That(inv.GetItemCount(1), Is.EqualTo(1));
        }

        [Test]
        public void RemoveItem_RemovesSlot_WhenQuantityReachesZero()
        {
            var inv = new Inventory();
            inv.AddItem(1, 2);
            var result = inv.RemoveItem(1, 2);
            Assert.That(result, Is.True);
            Assert.That(inv.Slots.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveItem_ReturnsFalse_WhenItemNotPresent()
        {
            var inv = new Inventory();
            Assert.That(inv.RemoveItem(99, 1), Is.False);
        }

        [Test]
        public void RemoveItem_ReturnsFalse_WhenInsufficientQuantity()
        {
            var inv = new Inventory();
            inv.AddItem(1, 1);
            var result = inv.RemoveItem(1, 5);
            Assert.That(result, Is.False);
            Assert.That(inv.GetItemCount(1), Is.EqualTo(1), "Quantity must not change on failed removal");
        }

        // ─── GetItemCount / HasItem ───────────────────────────────────────────

        [Test]
        public void GetItemCount_ReturnsCorrectQuantity()
        {
            var inv = new Inventory();
            inv.AddItem(1, 2);
            Assert.That(inv.GetItemCount(1), Is.EqualTo(2));
        }

        [Test]
        public void GetItemCount_ReturnsZero_WhenItemNotPresent()
        {
            Assert.That(new Inventory().GetItemCount(99), Is.EqualTo(0));
        }

        [Test]
        public void HasItem_ReturnsTrue_WhenItemPresent()
        {
            var inv = new Inventory();
            inv.AddItem(1, 1);
            Assert.That(inv.HasItem(1), Is.True);
        }

        [Test]
        public void HasItem_ReturnsFalse_WhenItemNotPresent()
        {
            Assert.That(new Inventory().HasItem(1), Is.False);
        }
    }
}
