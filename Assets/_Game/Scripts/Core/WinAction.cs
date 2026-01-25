using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles winning a battle
    /// </summary>
    public class WinAction : IGameAction
    {
        public ActionType Type => ActionType.Win;
        public Entity Actor { get; }
        public Entity Target { get; } 

        /// <param name="actor">winner</param>
        /// <param name="target">loser</param>
        public WinAction(Entity actor, Entity target)
        {
            Actor = actor;
            Target = target;
        }

        public void ApplyEffect()
        {
            // TODO: loot and xp happens here
        }
    }
}
