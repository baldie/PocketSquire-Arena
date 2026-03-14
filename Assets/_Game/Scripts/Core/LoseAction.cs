using System;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles losing a battle
    /// </summary>
    public class LoseAction : IGameAction
    {
        public ActionType Type => ActionType.Lose;
        public Entity Actor { get; }
        public Entity Target { get; } 

        /// <param name="actor">loser</param>
        /// <param name="target">winner</param>
        public LoseAction(Entity actor, Entity target)
        {
            Actor = actor;
            Target = target;
        }

        public void ApplyEffect()
        {
            if (Actor is Player player)
            {
                var context = new PerkContext { Player = player, Target = Target };
                PerkProcessor.ProcessEvent(PerkTriggerEvent.BattleLost, player, context);
            }
        }
    }
}
