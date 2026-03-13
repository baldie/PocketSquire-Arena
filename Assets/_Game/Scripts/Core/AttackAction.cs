#nullable enable
using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Represents a basic attack action where the actor deals damage to the target.
    /// Hit/miss and crit are resolved in the constructor so downstream code sees
    /// finalised, immutable results (same pattern as the existing Damage property).
    /// </summary>
    public class AttackAction : ResolvedAttackActionBase
    {
        public override ActionType Type => ActionType.Attack;
        protected override string MissText => $"{Actor.Name} attacked {Target.Name} but missed!";
        protected override string HitText => $"{Actor.Name} attacks {Target.Name} for ";
        protected override bool IsSpecialAttack => false;

        public AttackAction(Entity actor, Entity target, Random? rng = null)
            : base(actor, target, isSpecial: false, rng)
        {
        }
    }
}
