#nullable enable
using System;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Represents a basic attack action where the actor deals damage to the target.
    /// Hit/miss and crit are resolved in the constructor so downstream code sees
    /// finalised, immutable results (same pattern as the existing Damage property).
    /// </summary>
    public class AttackAction : IGameAction
    {
        public ActionType Type => ActionType.Attack;
        public Entity Actor { get; }
        public Entity Target { get; }

        /// <summary>The finalised damage after hit/crit resolution. 0 if the attack missed.</summary>
        public int Damage { get; private set; }

        public bool DidHit { get; }
        public bool IsCrit { get; }

        public AttackAction(Entity actor, Entity target, Random? rng = null)
        {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            rng ??= new Random();

            int baseDamage = Math.Max(1, actor.Attributes.Strength);

            // Hit chance: base 80%, ±2% per relative Dexterity, clamped 5–99
            int hitChance = 80 + (actor.Attributes.Dexterity - target.Attributes.Dexterity) * 2;
            hitChance = Math.Clamp(hitChance, 5, 99);
            DidHit = rng.Next(100) < hitChance;

            // Crit chance: base 5%, +1% per Luck above 5, clamped 1–50
            int critChance = 5 + Math.Max(0, actor.Attributes.Luck - 5);
            critChance = Math.Clamp(critChance, 1, 50);
            IsCrit = DidHit && rng.Next(100) < critChance;

            if (!DidHit)
                Damage = 0;
            else if (IsCrit)
                Damage = (int)(baseDamage * 1.5f);
            else
                Damage = baseDamage;
        }

        public void ApplyEffect()
        {
            if (!DidHit)
            {
                Console.WriteLine($"{Actor.Name} attacked {Target.Name} but missed!");
                return;
            }

            string critNote = IsCrit ? " (CRIT!)" : string.Empty;
            Console.WriteLine($"{Actor.Name} attacks {Target.Name} for {Damage} damage!{critNote}");

            // When targeting the player, supply a WouldDie callback so survival perks (e.g. Phoenix Heart)
            // can intercept the killing blow before HP reaches 0.
            if (Target is Player targetPlayer)
            {
                Target.TakeDamage(Damage, dmg =>
                {
                    var ctx = new PocketSquire.Arena.Core.Perks.PerkContext
                    {
                        Player = targetPlayer,
                        Damage = dmg,
                        PlayerHpPercent = targetPlayer.MaxHealth > 0
                            ? (int)(targetPlayer.Health * 100f / targetPlayer.MaxHealth) : 0
                    };
                    var r = PocketSquire.Arena.Core.Perks.PerkProcessor.ProcessEvent(
                        PocketSquire.Arena.Core.Perks.PerkTriggerEvent.WouldDie, targetPlayer, ctx);
                    return r.SurviveFatalBlow;
                });
            }
            else
            {
                Target.TakeDamage(Damage);
            }
        }
    }
}
