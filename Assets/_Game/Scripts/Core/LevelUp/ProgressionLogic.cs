#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSquire.Arena.Core.LevelUp
{
    public class ProgressionLogic
    {
        private readonly int[] _xpThresholds; // Index 0 = Lv1(=0), Index 1 = Lv2, etc.
        private readonly List<LevelReward> _rewards;

        // Legacy constructor for backward compatibility during refactor
        // Assumes the caller will fix the XP values if they were relying on LevelReward.ExperienceRequired
        public ProgressionLogic(IEnumerable<LevelReward> rewards)
        {
            _rewards = rewards.OrderBy(x => x.Level).ToList();
            
            // Build a fake XP table for compat if needed, or just default.
            // Since we removed ExperienceRequired from LevelReward, this constructor 
            // is tough to fully robustify without extra data.
            // Ideally we upgrade callers to use the new constructor.
            _xpThresholds = new int[0]; 
        }

        // New constructor
        public ProgressionLogic(int[] xpThresholds, IEnumerable<LevelReward> rewards)
        {
            _xpThresholds = xpThresholds;
            _rewards = rewards.OrderBy(x => x.Level).ToList();
        }

        public LevelReward GetRewardForLevel(int level)
        {
            var reward = _rewards.FirstOrDefault(x => x.Level == level);
            return reward ?? new LevelReward { Level = level }; // Empty reward if none defined
        }

        public int GetLevelForExperience(int experience)
        {
            if (_xpThresholds == null || _xpThresholds.Length == 0) return 1;

            // _xpThresholds index i corresponds to Level (i+1).
            // Level 1 = index 0 (0 XP).
            // Level 2 = index 1 (100 XP).
            
            // Binary search to find the highest threshold <= experience
            int index = Array.BinarySearch(_xpThresholds, experience);
            
            if (index >= 0)
            {
                // Exact match found.
                // Level = index + 1
                // But wait, if multiple levels share same XP (unlikely but possible override),
                // binary search returns *an* index.
                // We want the HIGHEST level reachable. 
                // Since thresholds should be monotonic, just checking next is enough.
                
                // Keep stepping forward if next level requires same XP (0-cost level)
                while (index < _xpThresholds.Length - 1 && _xpThresholds[index + 1] <= experience)
                {
                    index++;
                }

                return index + 1;
            }
            else
            {
                // BinarySearch returns bitwise complement of the index of the first element LARGER than value.
                // ~index is the insertion point.
                // insertionPoint is the first level we DO NOT qualify for.
                // So (insertionPoint - 1) is the index of the highest level we DO qualify for.
                
                int insertionPoint = ~index;
                int levelIndex = insertionPoint - 1;
                
                if (levelIndex < 0) return 1; // Should not happen if index 0 is 0 XP
                return levelIndex + 1;
            }
        }

        public int GetExperienceRequiredForLevel(int level)
        {
            if (_xpThresholds == null || _xpThresholds.Length == 0) return 0;
            
            int index = level - 1;
            if (index < 0) return 0;
            if (index >= _xpThresholds.Length) return int.MaxValue; // Cap at max
            
            return _xpThresholds[index];
        }

        // Convenience: XP needed to go from current XP to next level
        public int GetXpToNextLevel(int currentXp)
        {
            int currentLevel = GetLevelForExperience(currentXp);
            int nextLevel = currentLevel + 1;
            
            int nextXpReq = GetExperienceRequiredForLevel(nextLevel);
            if (nextXpReq == int.MaxValue) return 0; // Max level reached
            
            return Math.Max(0, nextXpReq - currentXp);
        }

        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (_xpThresholds == null || _xpThresholds.Length == 0)
            {
                errorMessage = "XP Thresholds are empty.";
                return false;
            }

            // Check monotonicity
            for (int i = 0; i < _xpThresholds.Length - 1; i++)
            {
                if (_xpThresholds[i] > _xpThresholds[i+1])
                {
                     errorMessage = $"Level {i+1} XP ({_xpThresholds[i]}) is higher than Level {i+2} ({_xpThresholds[i+1]}).";
                     return false;
                }
            }
            return true;
        }
    }
}
