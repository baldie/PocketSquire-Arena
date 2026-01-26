using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles changing turns
    /// </summary>
    public class ChangeTurnsAction : IGameAction
    {
        public ActionType Type => ActionType.ChangeTurns;
        public Entity Actor { get; }
        public Entity Target { get; }

        private Battle _battle;
        public ChangeTurnsAction(Battle? battle = null)
        {
            _battle = battle ?? GameWorld.Battle ?? throw new InvalidOperationException("No battle found in GameWorld.Battle");
            if (_battle.CurrentTurn != null) {
                Actor = _battle.CurrentTurn.Actor;
                Target = _battle.CurrentTurn.Target;
            }
        }

        public void ApplyEffect()
        {
            if (_battle.IsOver()) return;
            Console.WriteLine($"Changing turns");
            var player1 = _battle.Player1;
            var player2 = _battle.Player2;
            
            if (_battle.CurrentTurn.IsPlayerTurn)
            {
                player2.IsDefending = false; // Reset defend for the new actor
                _battle.CurrentTurn = new Turn(player2, player1);
            }
            else
            {
                player1.IsDefending = false; // Reset defend for the new actor
                _battle.CurrentTurn = new Turn(player1, player2);
            }
        }
    }
}
