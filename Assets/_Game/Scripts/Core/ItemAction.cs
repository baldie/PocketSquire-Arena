using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that handles using an item
    /// </summary>
    public class ItemAction : IGameAction
    {
        public ActionType Type => ActionType.Item;
        public Entity Actor { get; }
        public Entity Target { get; }

        public ItemAction()
        {
            var battle = GameWorld.Battle ?? throw new InvalidOperationException("No active battle");
            Actor = battle.CurrentTurn?.Actor ?? battle.Player1;
            Target = battle.CurrentTurn?.Target ?? battle.Player2;
        }

        public void ApplyEffect()
        {
            Console.WriteLine($"Using item");
        }
    }
}
