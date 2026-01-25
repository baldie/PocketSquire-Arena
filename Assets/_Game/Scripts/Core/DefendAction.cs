using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Action that puts the actor into a defensive stance, reducing incoming damage.
    /// </summary>
    public class DefendAction : IGameAction
    {
        public ActionType Type => ActionType.Defend;
        public Entity Actor { get; }
        public Entity Target { get; } // For defend, target is self or null, but we keep interface consistency

        public DefendAction(Entity actor)
        {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Target = actor; // Target acts on self
        }

        public void ApplyEffect()
        {
            Actor.IsDefending = true;
            Console.WriteLine($"{Actor.Name} is defending!");
        }
    }
}
