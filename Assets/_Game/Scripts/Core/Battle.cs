using System;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core
{
    public class Battle
    {
        public Turn CurrentTurn { get; private set; }
        public bool IsOver {
            get
            {
                return Player1.IsDead || Player2.IsDead;
            }
        }

        public Entity Player1 { get; private set; }
        public Entity Player2 { get; private set; }

        /// <summary>
        /// Creates a new battle. Player1 goes first. If both entities are monsters they fight each other
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        public Battle(Entity player1, Entity player2){
            if (player1 == null) throw new ArgumentNullException(nameof(player1));
            if (player2 == null) throw new ArgumentNullException(nameof(player2));
            this.Player1 = player1;
            this.Player2 = player2;
            Console.WriteLine("Creating new battle");
            CurrentTurn = new Turn(player1, player2, changeTurns);
        }

        private void changeTurns()
        {
            if (CurrentTurn.IsPlayerTurn)
            {
                Player2.IsDefending = false; // Reset defend for the new actor
                CurrentTurn = new Turn(Player2, Player1, changeTurns);
            }
            else
            {
                Player1.IsDefending = false; // Reset defend for the new actor
                CurrentTurn = new Turn(Player1, Player2, changeTurns);
            }
        }
    }
}