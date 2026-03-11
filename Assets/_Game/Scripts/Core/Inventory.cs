#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PocketSquire.Arena.Core.Perks;

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
        // Satchel perk IDs — matched against the player's Active Perks
        private const string SatchelTier1 = "satchel_tier_1";
        private const string SatchelTier2 = "satchel_tier_2";
        private const string SatchelTier3 = "satchel_tier_3";

        private const int BaseMaxSlots  = 2;
        private const int BaseMaxStack  = 2;

        public int MaxSlots     { get; private set; } = BaseMaxSlots;
        public int MaxStackSize { get; private set; } = BaseMaxStack;

        public List<InventorySlot> Slots = new();

        public static int CalculateCapacity(List<Perk> activePerks)
        {
            if (activePerks.Any(p => p != null && p.Id == SatchelTier3)) return 5;
            if (activePerks.Any(p => p != null && p.Id == SatchelTier2)) return 4;
            if (activePerks.Any(p => p != null && p.Id == SatchelTier1)) return 3;
            return BaseMaxSlots;
        }

        /// <summary>
        /// Recalculates capacity from the player's active perks.
        /// Call this whenever a satchel perk is granted so the inventory
        /// immediately reflects the new limits without losing existing items.
        /// </summary>
        public void UpdateCapacity(List<Perk> activePerks)
        {
            MaxSlots = CalculateCapacity(activePerks);
            
            if (MaxSlots == 5) MaxStackSize = 5;
            else if (MaxSlots == 4) MaxStackSize = 4;
            else if (MaxSlots == 3) MaxStackSize = 3;
            else MaxStackSize = BaseMaxStack;
        }

        /// <summary>
        /// Returns true if there is room to add at least one of the given item.
        /// Checks if any existing slot for the item has room, or if a new slot can be opened.
        /// </summary>
        public bool HasRoom(int itemId)
        {
            var existingWithRoom = Slots.FirstOrDefault(s => s.CanStack(itemId) && !s.IsFull(MaxStackSize));
            if (existingWithRoom != null) return true;
            return Slots.Count < MaxSlots;
        }

        /// <summary>
        /// Adds items one unit at a time. It adds to any existing matching stackable 
        /// items where there is room. Failing that, it looks for an existing free 
        /// inventory slot. Failing that, it fails. Items added before failure are kept.
        /// </summary>
        public bool AddItem(int itemId, int quantity = 1)
        {
            for (int i = 0; i < quantity; i++)
            {
                var existingWithRoom = Slots.FirstOrDefault(s => s.CanStack(itemId) && !s.IsFull(MaxStackSize));

                if (existingWithRoom != null)
                {
                    existingWithRoom.Quantity++;
                }
                else
                {
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
            int totalOwned = GetItemCount(itemId);
            if (totalOwned < quantity) return false;

            int remainingToRemove = quantity;
            for (int i = Slots.Count - 1; i >= 0; i--)
            {
                var slot = Slots[i];
                if (slot.ItemId == itemId)
                {
                    if (slot.Quantity <= remainingToRemove)
                    {
                        remainingToRemove -= slot.Quantity;
                        Slots.RemoveAt(i);
                    }
                    else
                    {
                        slot.Quantity -= remainingToRemove;
                        remainingToRemove = 0;
                    }

                    if (remainingToRemove == 0) break;
                }
            }

            return true;
        }

        /// <summary>Returns the total quantity of a specific item across all stacks in the inventory.</summary>
        public int GetItemCount(int itemId)
        {
            return Slots.Where(s => s.ItemId == itemId).Sum(s => s.Quantity);
        }

        /// <summary>Checks if the inventory contains at least one of the specified item.</summary>
        public bool HasItem(int itemId)
        {
            return Slots.Any(s => s.ItemId == itemId);
        }
    }
}
