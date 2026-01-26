using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles changing turns
    /// </summary>
    public class YieldAction : IGameAction
    {
        public ActionType Type => ActionType.Yield;
        public Entity Actor { get; }
        public Entity Target { get; }

        public YieldAction()
        {
            if (GameWorld.Battle.CurrentTurn != null) {
                Actor = GameWorld.Battle.CurrentTurn.Actor;
                Target = GameWorld.Battle.CurrentTurn.Target;
            }
        }

        public void ApplyEffect()
        {
            Console.WriteLine($"{Actor.Name} yields!");
        }
    }
}
