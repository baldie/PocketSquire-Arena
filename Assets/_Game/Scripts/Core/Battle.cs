using System;

namespace PocketSquire.Arena.Core
{
    public class Battle
    {
        public Turn? CurrentTurn { get; set; }
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
            CurrentTurn = new Turn(player1, player2);
        }

        public bool IsOver()
        {
            return CurrentTurn == null || Player1.IsDefeated || Player2.IsDefeated;
        }

        public void AdvanceTurn()
        {
            var action = new ChangeTurnsAction(this);
            action.ApplyEffect();
        }

        /// <summary>
        /// Determines the next action to be executed
        /// </summary>
        /// <param name="action">The action that was just completed</param>
        /// <returns>The next action to be executed, or null if the battle is over</returns>
        public IGameAction? DetermineNextAction(IGameAction action)
        {
            // End of battle logic
            var someoneHasLost = Player1.IsDefeated || Player2.IsDefeated;
            var battleIsStillGoing = CurrentTurn != null;
            if (someoneHasLost && battleIsStillGoing)
            {
                CurrentTurn = null;
                return Player1.IsDefeated
                    ? (IGameAction)new LoseAction(Player1, Player2)
                    : (IGameAction)new WinAction(Player1, Player2);
            }

            if (battleIsStillGoing && CurrentTurn != null)
            {
                // If it's the monster's turn, let it take an action
                IGameAction? nextAction = null;
                if (action.Type == ActionType.ChangeTurns && CurrentTurn.Actor is Monster monster) {
                    switch(monster.DetermineAction(CurrentTurn.Target)) {
                        case ActionType.Attack:
                            nextAction = new AttackAction(CurrentTurn.Actor, CurrentTurn.Target);
                            break;
                        case ActionType.SpecialAttack:
                            nextAction = new SpecialAttackAction(CurrentTurn.Actor, CurrentTurn.Target);
                            break;
                        case ActionType.Defend:
                            nextAction = new DefendAction(CurrentTurn.Actor);
                            break;
                        case ActionType.Yield:
                            nextAction = new ChangeTurnsAction(this);
                            break;
                        default:
                            Console.WriteLine("Monster took an invalid action");
                            break;
                    }
                }
                if (nextAction != null) return nextAction;
            }

            return action.Type != ActionType.ChangeTurns ? new ChangeTurnsAction(this) : null;
        }
    }
}