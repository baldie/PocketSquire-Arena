

using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles using an item
    /// </summary>
    public class ItemAction : IGameAction
    {
        public ActionType Type => ActionType.Item;
        public Entity Actor { get; }
        public Entity Target { get; }
        public int ItemId { get; }
        public Item ItemData { get; }

        public ItemAction(int itemId)
        {
            var battle = GameState.Battle ?? throw new InvalidOperationException("No active battle");
            Actor = battle.CurrentTurn?.Actor ?? battle.Player1;
            Target = battle.CurrentTurn?.Target ?? battle.Player2;
            
            ItemId = itemId;
            ItemData = GameWorld.GetItemById(itemId) ?? throw new InvalidOperationException($"Item with id {itemId} not found");
        }

        public void ApplyEffect()
        {
            Console.WriteLine($"Using item: {ItemData.Name}");
            
            // Apply item effect based on type
            if (IsHealthPotion(ItemData))
            {
                ApplyHealing_HealthPotion(Actor, ItemData);
            }
        }

        private bool IsHealthPotion(Item item)
        {
            return item.Name.Contains("Health Potion", StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyHealing_HealthPotion(Entity actor, Item item)
        {
            int healAmount = 50; // Default flat

            if (item.Description.Contains("%"))
            {
                // Simple parsing for "Heals X%" - defaulting to 25% for now
                healAmount = (int)(actor.MaxHealth * 0.25f);
            }

            actor.Heal(healAmount);
            Console.WriteLine($"Healed {actor.Name} for {healAmount} HP. Current HP: {actor.Health}/{actor.MaxHealth}");
            
            // CRITICAL: Remove item from inventory AFTER effect is applied
            bool removed = Actor.Inventory.RemoveItem(ItemId, 1);
            if (!removed)
            {
                // In Unity this would remain internal logic, logging handled by caller/wrapper if needed
                Console.WriteLine($"Failed to remove item {ItemId} from inventory");
            }
            else
            {
                Console.WriteLine($"Removed 1x {ItemData.Name} from inventory");
            }
        }
    }
}
