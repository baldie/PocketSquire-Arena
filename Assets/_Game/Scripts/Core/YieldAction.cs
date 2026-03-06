#nullable enable
using System;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles the player yielding (attempting to flee the battle).
    /// Fires PlayerAttemptedYield perk event (YieldBonus modifier is readable by callers).
    /// </summary>
    public class YieldAction : IGameAction
    {
        public ActionType Type => ActionType.Yield;
        public Entity Actor { get; }
        public Entity Target { get; }

        public YieldAction()
        {
            var battle = GameState.Battle ?? throw new InvalidOperationException("No active battle");
            Actor = battle.CurrentTurn?.Actor ?? battle.Player1;
            Target = battle.CurrentTurn?.Target ?? battle.Player2;
        }

        public void ApplyEffect()
        {
            Console.WriteLine($"{Actor.Name} yields!");

            if (Actor is Player player)
            {
                var context = new PerkContext { Player = player };
                var result = PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerAttemptedYield, player, context);
                if (result.YieldChanceBonus > 0)
                    Console.WriteLine($"[Perk] Yield chance boosted by {result.YieldChanceBonus}%.");
            }
        }
    }
}
