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
    }

    [Serializable]
    public class Inventory
    {
        public List<InventorySlot> Slots = new();

        /// <summary>
        /// Adds an item to the inventory. If the item already exists, increases quantity.
        /// </summary>
        public void AddItem(int itemId, int quantity = 1)
        {
            var existingSlot = Slots.FirstOrDefault(s => s.ItemId == itemId);
            
            if (existingSlot != null)
            {
                existingSlot.Quantity += quantity;
            }
            else
            {
                Slots.Add(new InventorySlot { ItemId = itemId, Quantity = quantity });
            }
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

        /// <summary>
        /// Returns the quantity of a specific item in the inventory.
        /// </summary>
        public int GetItemCount(int itemId)
        {
            var slot = Slots.FirstOrDefault(s => s.ItemId == itemId);
            return slot?.Quantity ?? 0;
        }

        /// <summary>
        /// Checks if the inventory contains at least one of the specified item.
        /// </summary>
        public bool HasItem(int itemId)
        {
            return Slots.Any(s => s.ItemId == itemId);
        }
    }
}
