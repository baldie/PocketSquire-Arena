#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class InventorySlot
    {
        public int ItemId;
        public int Quantity;

        /// <summary>Returns true if this slot has reached the stack limit.</summary>
        public bool IsFull(int maxStack) => Quantity >= maxStack;

        /// <summary>Returns true if this slot holds the given item and can accept more.</summary>
        public bool CanStack(int itemId) => ItemId == itemId;
    }

    [Serializable]
    public class Inventory
    {
        // Satchel perk IDs — matched against the player's UnlockedPerks
        private const string SatchelTier1 = "satchel_tier_1";
        private const string SatchelTier2 = "satchel_tier_2";
        private const string SatchelTier3 = "satchel_tier_3";

        private const int BaseMaxSlots  = 2;
        private const int BaseMaxStack  = 2;

        public int MaxSlots     { get; private set; } = BaseMaxSlots;
        public int MaxStackSize { get; private set; } = BaseMaxStack;

        public List<InventorySlot> Slots = new();

        /// <summary>
        /// Recalculates capacity from the player's unlocked perks.
        /// Call this whenever a satchel perk is granted so the inventory
        /// immediately reflects the new limits without losing existing items.
        /// </summary>
        public void UpdateCapacity(HashSet<string> ownedPerks)
        {
            if (ownedPerks.Contains(SatchelTier3))
            {
                MaxSlots = 5; MaxStackSize = 5;
            }
            else if (ownedPerks.Contains(SatchelTier2))
            {
                MaxSlots = 4; MaxStackSize = 4;
            }
            else if (ownedPerks.Contains(SatchelTier1))
            {
                MaxSlots = 3; MaxStackSize = 3;
            }
            else
            {
                MaxSlots = BaseMaxSlots; MaxStackSize = BaseMaxStack;
            }
        }

        /// <summary>
        /// Returns true if there is room to add at least one of the given item.
        /// Each item type occupies exactly one slot; checks if that slot has room
        /// or if a new slot can be opened.
        /// </summary>
        public bool HasRoom(int itemId)
        {
            var existing = Slots.FirstOrDefault(s => s.CanStack(itemId));
            if (existing != null) return !existing.IsFull(MaxStackSize);
            return Slots.Count < MaxSlots;
        }

        /// <summary>
        /// Adds items one unit at a time, stacking onto the item's existing slot first.
        /// Each item type occupies exactly one slot; once that slot is full, further
        /// additions are rejected. Returns false as soon as a unit cannot be placed.
        /// Items added before the failure are kept.
        /// </summary>
        public bool AddItem(int itemId, int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                var existing = Slots.FirstOrDefault(s => s.CanStack(itemId));

                if (existing != null)
                {
                    // Item already has a slot — stack if room, otherwise reject
                    if (existing.IsFull(MaxStackSize)) return false;
                    existing.Quantity++;
                }
                else
                {
                    // No slot yet — open one if capacity allows
                    if (Slots.Count >= MaxSlots) return false;
                    Slots.Add(new InventorySlot { ItemId = itemId, Quantity = 1 });
                }
            }
            return true;
        }

        /// <summary>
        /// Removes an item from the inventory. Returns false if item not present or insufficient quantity.
        /// </summary>
        public bool RemoveItem(int itemId, int quantity = 1)
        {
            var existingSlot = Slots.FirstOrDefault(s => s.ItemId == itemId);

            if (existingSlot == null || existingSlot.Quantity < quantity)
            {
                return false;
            }

            existingSlot.Quantity -= quantity;

            if (existingSlot.Quantity == 0)
            {
                Slots.Remove(existingSlot);
            }

            return true;
        }

        /// <summary>Returns the quantity of a specific item in the inventory.</summary>
        public int GetItemCount(int itemId)
        {
            var slot = Slots.FirstOrDefault(s => s.ItemId == itemId);
            return slot?.Quantity ?? 0;
        }

        /// <summary>Checks if the inventory contains at least one of the specified item.</summary>
        public bool HasItem(int itemId)
        {
            return Slots.Any(s => s.ItemId == itemId);
        }
    }
}
