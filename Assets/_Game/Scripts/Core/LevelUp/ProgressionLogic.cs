#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSquire.Arena.Core.LevelUp
{
    public class ProgressionLogic
    {
        private readonly List<LevelReward> _schedule;

        public ProgressionLogic(IEnumerable<LevelReward> schedule)
        {
            _schedule = schedule.OrderBy(x => x.Level).ToList();
        }

        public LevelReward GetRewardForLevel(int level)
        {
            var reward = _schedule.FirstOrDefault(x => x.Level == level);
            return reward ?? new LevelReward { Level = level };
        }

        public int GetLevelForExperience(int experience)
        {
            if (_schedule.Count == 0) return 1;

            var reachedLevel = _schedule
                .Where(x => experience >= x.ExperienceRequired)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            return reachedLevel?.Level ?? 1;
        }

        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            for (int i = 0; i < _schedule.Count - 1; i++)
            {
                var current = _schedule[i];
                var next = _schedule[i + 1];

                if (next.ExperienceRequired <= current.ExperienceRequired)
                {
                    errorMessage = $"Level {next.Level} XP requirement ({next.ExperienceRequired}) must be higher than Level {current.Level} ({current.ExperienceRequired}).";
                    return false;
                }

                if (next.Level != current.Level + 1)
                {
                   // Optional: Check for gaps in levels if you want them strictly sequential
                }
            }

            return true;
        }
    }
}
