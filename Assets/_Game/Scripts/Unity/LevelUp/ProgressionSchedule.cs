#nullable enable
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using PocketSquire.Arena.Core.LevelUp;

namespace PocketSquire.Arena.Unity.LevelUp
{
    [CreateAssetMenu(fileName = "ProgressionSchedule", menuName = "PocketSquire/LevelUp/ProgressionSchedule")]
    public class ProgressionSchedule : ScriptableObject
    {
        [Serializable]
        public class LevelConfig
        {
            public int level;
            public int experienceRequired;
            public int statPoints;
            public List<PerkNode> perkChoices = new List<PerkNode>();
        }

        [SerializeField] private List<LevelConfig> schedule = new List<LevelConfig>();

        private ProgressionLogic? _logic;
        public ProgressionLogic Logic => _logic ??= new ProgressionLogic(schedule.Select(c => new LevelReward
        {
            Level = c.level,
            ExperienceRequired = c.experienceRequired,
            StatPoints = c.statPoints,
            PerkChoices = c.perkChoices.Where(p => p != null).Select(p => p.Id).ToList()
        }));

        public int GetLevelForExperience(int experience) => Logic.GetLevelForExperience(experience);

        public LevelReward GetRewardForLevel(int level) => Logic.GetRewardForLevel(level);

        private void OnValidate()
        {
            _logic = null; // Clear cache
            if (!Logic.IsValid(out string error))
            {
                Debug.LogWarning($"[ProgressionSchedule] {name}: {error}");
            }
        }
    }
}
