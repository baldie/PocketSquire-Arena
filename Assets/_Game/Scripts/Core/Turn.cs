using System;
namespace PocketSquire.Arena.Core
{
    public class Turn{
        public Entity Actor { get; private set; }
        public Entity Target { get; private set; }
        private Action changeTurns;

        public bool IsPlayerTurn
        {
            get
            {
                return Actor is Player;
            }
        }

        public Turn(Entity actor, Entity target, Action changeTurns)
        {
            if (actor == null) throw new ArgumentNullException(nameof(actor));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (changeTurns == null) throw new ArgumentNullException(nameof(changeTurns));
            this.Actor = actor;
            this.Target = target;
            this.changeTurns = changeTurns;
        }

        public void End()
        {
            if (this.changeTurns != null)
            {
                this.changeTurns();
            }
        }

        public void Execute()
        {
            if (IsPlayerTurn)
            {
                Console.WriteLine("IsPlayerTurn == true when battle.execute() called. This should never happen!");
                return;
            }

            // TODO: Implement monster turn
            Console.WriteLine(Actor.Name + " attacks " + Target.Name);

            if (this.changeTurns != null)
            {
                this.changeTurns();
            }
        }
    }
}