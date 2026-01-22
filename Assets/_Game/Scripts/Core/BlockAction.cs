using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that puts the actor into a defensive stance, reducing incoming damage.
    /// </summary>
    public class BlockAction : IGameAction
    {
        public ActionType Type => ActionType.Block;
        public Entity Actor { get; }
        public Entity Target { get; } // For block, target is self or null, but we keep interface consistency

        public BlockAction(Entity actor)
        {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Target = actor; // Target acts on self
        }

        public void ApplyEffect()
        {
            Actor.IsBlocking = true;
            Console.WriteLine($"{Actor.Name} is defending!");
        }
    }
}
