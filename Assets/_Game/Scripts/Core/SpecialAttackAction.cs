using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Represents a special attack action where the actor deals agility-based damage to the target.
    /// Used by monsters approximately 25% of the time instead of a regular attack.
    /// </summary>
    public class SpecialAttackAction : IGameAction
    {
        public ActionType Type => ActionType.SpecialAttack;
        public Entity Actor { get; }
        public Entity Target { get; }
        
        /// <summary>
        /// The amount of damage this special attack will deal.
        /// </summary>
        public int Damage { get; }

        public SpecialAttackAction(Entity actor, Entity target)
        {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Damage = CalculateDamage(actor, target);
        }

        private int CalculateDamage(Entity attacker, Entity target)
        {
            int baseDamage = attacker.Attributes.Strength;
            return Math.Max(1, baseDamage);
        }

        public void ApplyEffect()
        {
            Target.TakeDamage(Damage);
            Console.WriteLine($"{Actor.Name} uses a special attack on {Target.Name} for {Damage} damage!");
        }
    }
}
