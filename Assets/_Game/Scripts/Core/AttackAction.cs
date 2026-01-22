using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Represents a basic attack action where the actor deals damage to the target.
    /// </summary>
    public class AttackAction : IGameAction
    {
        public ActionType Type => ActionType.Attack;
        public Entity Actor { get; }
        public Entity Target { get; }
        
        /// <summary>
        /// The amount of damage this attack will deal.
        /// </summary>
        public int Damage { get; }

        public AttackAction(Entity actor, Entity target, int damage)
        {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Damage = damage;
        }

        public void ApplyEffect()
        {
            Target.TakeDamage(Damage);
            Console.WriteLine($"{Actor.Name} attacks {Target.Name} for {Damage} damage!");
        }
    }
}
