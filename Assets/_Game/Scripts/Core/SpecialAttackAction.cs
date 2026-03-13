#nullable enable
using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Represents a special attack action where the actor deals agility-based damage to the target.
    /// Used by monsters approximately 25% of the time instead of a regular attack.
    /// Hit/miss and crit are resolved in the constructor for immutability.
    /// </summary>
    public class SpecialAttackAction : ResolvedAttackActionBase
    {
        public override ActionType Type => ActionType.SpecialAttack;
        protected override string MissText => $"{Actor.Name} used a special attack on {Target.Name} but missed!";
        protected override string HitText => $"{Actor.Name} uses a special attack on {Target.Name} for ";
        protected override bool IsSpecialAttack => true;

        public SpecialAttackAction(Entity actor, Entity target, Random? rng = null)
            : base(ValidateActorCanUseSpecialAttack(actor), target, isSpecial: true, rng)
        {
        }

        public override void ApplyEffect()
        {
            if (Actor is Player playerActor)
            {
                if (!playerActor.TrySpendManaForSpecialAttack())
                {
                    throw new InvalidOperationException($"{playerActor.Name} cannot afford a special attack.");
                }

                Console.WriteLine($"{Actor.Name} spends {playerActor.SpecialAttackManaCost} mana. Remaining: {playerActor.Mana}/{playerActor.MaxMana}");
            }

            base.ApplyEffect();
        }

        private static Entity ValidateActorCanUseSpecialAttack(Entity actor)
        {
            if (actor is Player playerActor && !playerActor.CanAffordSpecialAttack())
            {
                throw new InvalidOperationException($"{playerActor.Name} cannot afford a special attack.");
            }

            return actor;
        }
    }
}
