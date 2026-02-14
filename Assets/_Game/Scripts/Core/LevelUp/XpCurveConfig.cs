#nullable enable
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.LevelUp
{
    public class XpCurveConfig
    {
        // --- Formula Params ---
        public int BaseXp { get; set; } = 100;      // XP needed for Level 2 (or first level using formula)
        public float Exponent { get; set; } = 1.5f;  // Power curve steepness (1 = linear, 2 = quadratic)

        // --- Bounds ---
        public int MaxLevel { get; set; } = 50;

        public XpCurveConfig() { }

        public XpCurveConfig(int baseXp, float exponent, int maxLevel)
        {
            BaseXp = baseXp;
            Exponent = exponent;
            MaxLevel = maxLevel;
        }
    }
}
