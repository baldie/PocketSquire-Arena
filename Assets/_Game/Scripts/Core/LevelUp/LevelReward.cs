#nullable enable
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.LevelUp
{
    public class LevelReward
    {
        public int Level { get; set; }
        // ExperienceRequired removed - now handled by XpCurveGenerator + ProgressionLogic
        public int StatPoints { get; set; }
        
        // --- Perk Rewards ---
        // Guaranteed specific perks (legacy behavior)
        public List<string> FixedPerkIds { get; set; } = new List<string>();

        // New: Dynamic pool draw
        public string? PerkPoolTag { get; set; }  // e.g. "tier1_pool"
        public int PerkPoolDrawCount { get; set; } // How many to draw from pool

        // Backward compatibility property for existing code that uses PerkChoices
        // We map it to FixedPerkIds
        public List<string> PerkChoices 
        { 
            get => FixedPerkIds; 
            set => FixedPerkIds = value; 
        }

        public LevelReward() { }

        public LevelReward(int level, int statPoints, List<string>? fixedPerkIds = null)
        {
            Level = level;
            StatPoints = statPoints;
            FixedPerkIds = fixedPerkIds ?? new List<string>();
        }
    }
}
