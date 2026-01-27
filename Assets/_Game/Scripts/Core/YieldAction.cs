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
            var battle = GameWorld.Battle ?? throw new InvalidOperationException("No active battle");
            Actor = battle.CurrentTurn?.Actor ?? battle.Player1;
            Target = battle.CurrentTurn?.Target ?? battle.Player2;
        }

        public void ApplyEffect()
        {
            Console.WriteLine($"{Actor.Name} yields!");
        }
    }
}
