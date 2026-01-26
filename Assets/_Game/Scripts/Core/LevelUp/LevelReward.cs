using System.Collections.Generic;

namespace PocketSquire.Arena.Core.LevelUp
{
    public class LevelReward
    {
        public int Level { get; set; }
        public int ExperienceRequired { get; set; }
        public int StatPoints { get; set; }
        public List<string> PerkChoices { get; set; } = new List<string>();

        public LevelReward() { }

        public LevelReward(int level, int experienceRequired, int statPoints, List<string>? perkChoices = null)
        {
            Level = level;
            ExperienceRequired = experienceRequired;
            StatPoints = statPoints;
            PerkChoices = perkChoices ?? new List<string>();
        }
    }
}
