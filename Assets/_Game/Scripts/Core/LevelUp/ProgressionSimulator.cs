#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSquire.Arena.Core.LevelUp
{
    public class ProgressionSimulator
    {
        private readonly ProgressionLogic _logic;
        
        public struct SimulationResult
        {
            public int LevelReached;
            public int TotalXpGained;
            public List<string> SelectedPerks;
            public Dictionary<int, int> XpAtLevelUp; // Level -> Total XP when reached
        }

        public ProgressionSimulator(ProgressionLogic logic)
        {
            _logic = logic;
        }

        /// <summary>
        /// Simulates a run where the player gains a fixed amount of XP per "step" until a target level is reached or max steps exceeded.
        /// </summary>
        public SimulationResult SimulateRun(int targetLevel, int xpPerStep, int maxSteps = 1000)
        {
            var result = new SimulationResult
            {
                LevelReached = 1,
                TotalXpGained = 0,
                SelectedPerks = new List<string>(),
                XpAtLevelUp = new Dictionary<int, int>()
            };

            int currentLevel = 1;
            int currentXp = 0;

            // Initialize level 1 state
            result.XpAtLevelUp[1] = 0;

            for (int step = 0; step < maxSteps; step++)
            {
                currentXp += xpPerStep;
                result.TotalXpGained = currentXp;

                int calculatedLevel = _logic.GetLevelForExperience(currentXp);

                // Check for level up
                if (calculatedLevel > currentLevel)
                {
                    // Handle multiple levels gained in one step
                    for (int l = currentLevel + 1; l <= calculatedLevel; l++)
                    {
                        result.XpAtLevelUp[l] = currentXp;
                        
                        // Simulate reward collection (simplified - just grab fixed perks for now)
                        var reward = _logic.GetRewardForLevel(l);
                        if (reward.FixedPerkIds != null)
                        {
                            result.SelectedPerks.AddRange(reward.FixedPerkIds);
                        }
                        // Note: Dynamic perks require a pool provider and RNG, which we might want to dependency inject if needed for advanced simulation.
                    }
                    currentLevel = calculatedLevel;
                }

                if (currentLevel >= targetLevel)
                {
                    break;
                }
            }

            result.LevelReached = currentLevel;
            return result;
        }
        
        /// <summary>
        /// Calculates the total XP required to reach a specific level directly from logic.
        /// Useful for quick balancing checks.
        /// </summary>
        public int GetXpToReachLevel(int level)
        {
            // Level 1 is 0 XP. 
            // Level 2 requires thresholds[0] XP.
            return _logic.GetExperienceRequiredForLevel(level);
        }
    }
}
