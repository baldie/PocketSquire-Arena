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
            if (GameWorld.Battle.CurrentTurn != null) {
                Actor = GameWorld.Battle.CurrentTurn.Actor;
                Target = GameWorld.Battle.CurrentTurn.Target;
            }
        }

        public void ApplyEffect()
        {
            Console.WriteLine($"Using item");
        }
    }
}
