#nullable enable
using System;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles using an item.
    /// Fires PlayerUsedItem perk event after applying the item effect.
    /// </summary>
    public class ItemAction : IGameAction
    {
        private const int SmallHealthPotionId = 1;
        private const int MediumHealthPotionId = 2;
        private const int LargeHealthPotionId = 3;

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

            // Fire perk event after item effect
            if (Actor is Player player)
            {
                var context = new PerkContext { Player = player };
                PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerUsedItem, player, context);
            }
        }

        private bool IsHealthPotion(Item item)
        {
            return item.Name.Contains("Health Potion", StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyHealing_HealthPotion(Entity actor, Item item)
        {
            int healAmount = 10; // Default flat value

            switch (ItemId)
            {
                case SmallHealthPotionId:
                    healAmount = (int)(actor.MaxHealth * 0.25f);
                    break;
                case MediumHealthPotionId:
                    healAmount = (int)(actor.MaxHealth * 0.50f);
                    break;
                case LargeHealthPotionId:
                    healAmount = (int)(actor.MaxHealth * 0.75f);
                    break;
            }

            actor.Heal(healAmount);
            Console.WriteLine($"Healed {actor.Name} for {healAmount} HP. Current HP: {actor.Health}/{actor.MaxHealth}");

            // CRITICAL: Remove item from inventory AFTER effect is applied
            bool removed = Actor.Inventory.RemoveItem(ItemId, 1);
            if (!removed)
            {
                Console.WriteLine($"Failed to remove item {ItemId} from inventory");
            }
            else
            {
                Console.WriteLine($"Removed 1x {ItemData.Name} from inventory");
            }
        }
    }
}
