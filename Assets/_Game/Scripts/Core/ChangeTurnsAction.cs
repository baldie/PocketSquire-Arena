#nullable enable
using System;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles changing turns in a battle.
    /// Also ticks duration on active perk states and fires PlayerTurnStarted/Ended events.
    /// </summary>
    public class ChangeTurnsAction : IGameAction
    {
        public ActionType Type => ActionType.ChangeTurns;
        public Entity Actor { get; }
        public Entity Target { get; }

        private Battle _battle;
        public ChangeTurnsAction(Battle? battle = null)
        {
            _battle = battle ?? GameState.Battle ?? throw new InvalidOperationException("No battle found in GameState.Battle");
            Actor = _battle.CurrentTurn?.Actor ?? _battle.Player1;
            Target = _battle.CurrentTurn?.Target ?? _battle.Player2;
        }

        public void ApplyEffect()
        {
            if (_battle.IsOver()) return;
            Console.WriteLine("Changing turns");
            var player1 = _battle.Player1;
            var player2 = _battle.Player2;

            bool wasPlayerTurn = _battle.CurrentTurn!.IsPlayerTurn;

            if (wasPlayerTurn)
            {
                player2.IsDefending = false;
                _battle.CurrentTurn = new Turn(player2, player1);
            }
            else
            {
                player1.IsDefending = false;
                _battle.CurrentTurn = new Turn(player1, player2);
            }

            // Tick down duration on player perk states each turn
            if (player1 is Player p)
            {
                PerkProcessor.TickDuration(p);
                if (p.UsesMana)
                {
                    p.RestoreMana(p.ManaRegenPerTurn);
                }

                // Fire PlayerTurnStarted / PlayerTurnEnded
                var ctx = new PerkContext { Player = p };
                if (!wasPlayerTurn)
                {
                    // Monster turn just ended → player turn starting
                    PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerTurnStarted, p, ctx);
                }
                else
                {
                    // Player turn just ended
                    PerkProcessor.ProcessEvent(PerkTriggerEvent.PlayerTurnEnded, p, ctx);
                }
            }
        }
    }
}
