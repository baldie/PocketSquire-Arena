using System;

namespace PocketSquire.Arena.Core
{
    [Serializable]
    public class Turn{
        public Entity Actor { get; private set; }
        public Entity Target { get; private set; }

        public bool IsPlayerTurn
        {
            get
            {
                return Actor is Player;
            }
        }

        public Turn(Entity actor, Entity target)
        {
            if (actor == null) throw new ArgumentNullException(nameof(actor));
            if (target == null) throw new ArgumentNullException(nameof(target));
            this.Actor = actor;
            this.Target = target;
        }
    }
}