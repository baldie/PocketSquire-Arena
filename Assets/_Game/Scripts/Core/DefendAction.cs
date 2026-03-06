#nullable enable
using System;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that puts the actor into a defensive stance, reducing incoming damage.
    /// </summary>
    public class DefendAction : IGameAction
    {
        public ActionType Type => ActionType.Defend;
        public Entity Actor { get; }
        public Entity Target { get; } // For defend, target is self

        public DefendAction(Entity actor)
        {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Target = actor;
        }

        public void ApplyEffect()
        {
            Actor.IsDefending = true;
            Console.WriteLine($"{Actor.Name} is defending!");

            // Only trigger perks for the player, not monsters
            if (Actor is Player player)
            {
                var context = new PerkContext { Player = player };
                PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerDefended, player, context);
            }
        }
    }
}
