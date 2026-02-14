#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSquire.Arena.Core.LevelUp
{
    public static class XpCurveGenerator
    {
        /// <summary>
        /// Generates the full cumulative XP schedule for levels 1 to MaxLevel.
        /// Index 0 corresponds to Level 1 (always 0 XP).
        /// Index 1 corresponds to Level 2, etc.
        /// </summary>
        public static int[] Generate(XpCurveConfig config)
        {
            if (config.MaxLevel < 1) return new int[] { 0 };

            var schedule = new int[config.MaxLevel]; // Index 0 = Level 1, Index MaxLevel-1 = Level MaxLevel
            schedule[0] = 0; // Level 1 always starts at 0 XP

            // Track cumulative XP for non-delta calculations
            long currentCumulative = 0;

            for (int lvl = 2; lvl <= config.MaxLevel; ++lvl)
            {
                int arrayIndex = lvl - 1;
                
                // Delta formula: Base * ((Level-1) ^ Exponent)
                // At Level 2: Base * 1^Exp = Base
                // At Level 3: Base * 2^Exp
                double val = config.BaseXp * Math.Pow(lvl - 1, config.Exponent);
                long xpForThisLevel = currentCumulative + (long)val;

                // Safety clamp to int.MaxValue and ensure monotonicity
                if (xpForThisLevel > int.MaxValue) xpForThisLevel = int.MaxValue;
                if (xpForThisLevel < currentCumulative) xpForThisLevel = currentCumulative; // Should strictly increase, but <= is safer for now

                schedule[arrayIndex] = (int)xpForThisLevel;
                currentCumulative = xpForThisLevel;
            }

            return schedule;
        }
    }
}
