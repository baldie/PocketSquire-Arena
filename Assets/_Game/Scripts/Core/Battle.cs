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
                return player1.IsDead || player2.IsDead;
            }
        }

        private Entity player1;
        private Entity player2;

        /// <summary>
        /// Creates a new battle. Player1 goes first. If both entities are monsters they fight each other
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        public Battle(Entity player1, Entity player2){
            this.player1 = player1;
            this.player2 = player2;
            Console.WriteLine("Creating new battle");
            CurrentTurn = new Turn(player1, player2, changeTurns);
        }

        private void changeTurns()
        {
            if (CurrentTurn.IsPlayerTurn)
            {
                CurrentTurn = new Turn(player2, player1, changeTurns);
            }
            else
            {
                CurrentTurn = new Turn(player1, player2, changeTurns);
            }
        }
    }
}