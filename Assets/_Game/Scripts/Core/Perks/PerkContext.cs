#nullable enable
using System;

namespace PocketSquire.Arena.Core.Perks
{
    /// <summary>
    /// Carries contextual data about the current game action into PerkProcessor.
    /// Passed by value so processors cannot mutate shared state accidentally.
    /// </summary>
    public class PerkContext
    {
        /// <summary>Damage that would be dealt / was dealt this action.</summary>
        public int Damage { get; set; }

        /// <summary>HP percentage of the player (0–100).</summary>
        public int PlayerHpPercent { get; set; }

        /// <summary>HP percentage of the monster target (0–100).</summary>
        public int TargetHpPercent { get; set; }

        /// <summary>Whether the triggering attack hit its target.</summary>
        public bool DidHit { get; set; }

        /// <summary>Whether the triggering attack was a critical hit.</summary>
        public bool IsCrit { get; set; }

        /// <summary>Seeded RNG for deterministic proc rolls (injectable for tests).</summary>
        public Random Rng { get; set; } = new Random();

        /// <summary>The player entity (for reading stats or modifying health).</summary>
        public Player? Player { get; set; }

        /// <summary>The opponent entity.</summary>
        public Entity? Target { get; set; }
    }
}
