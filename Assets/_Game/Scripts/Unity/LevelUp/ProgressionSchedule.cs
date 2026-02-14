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
        // --- Core XP Curve Fields ---
        [Header("XP Curve Config")]
        [SerializeField] private int baseXp = 100;
        [SerializeField] private float exponent = 1.6f;
        [SerializeField] private int maxLevel = 50;

        // --- Rewards ---
        [Header("Rewards")]
        [SerializeField] private List<LevelRewardEntry> rewards = new List<LevelRewardEntry>();

        // --- Perk Pools ---
        [Header("Perk Pools")]
        [SerializeField] private List<PerkPoolEntry> perkPools = new List<PerkPoolEntry>();

        // Helper classes for Unity serialization
        [Serializable]
        public class LevelRewardEntry
        {
            public int level;
            public int statPoints;
            public List<PerkNode> perkChoices = new List<PerkNode>();
            
            [Tooltip("Tag of the Perk Pool to draw from. Leave empty for none.")]
            public string perkPoolTag = String.Empty;
            public int perkPoolDrawCount = 3;
        }

        [Serializable]
        public class PerkPoolEntry
        {
            public string tag;
            public List<PerkNode> perks = new List<PerkNode>();
        }

        public Dictionary<string, PerkPool> RuntimePerkPools { get; private set; } = new Dictionary<string, PerkPool>();

        private ProgressionLogic? _logic;
        public ProgressionLogic Logic 
        {
            get
            {
                if (_logic == null)
                {
                    BuildLogic();
                }
                return _logic!;
            }
        }

        private void BuildLogic()
        {
            // 1. Build Config
            var config = new XpCurveConfig(baseXp, exponent, maxLevel);
            
            // 2. Generate Schedule
            int[] thresholds = XpCurveGenerator.Generate(config);

            // 3. Build Perk Pools
            RuntimePerkPools.Clear();
            foreach (var poolEntry in perkPools)
            {
                if (string.IsNullOrEmpty(poolEntry.tag)) continue;
                
                var corePerks = poolEntry.perks
                    .Where(p => p != null)
                    .Select(p => p.ToCorePerk())
                    .ToList();
                
                var pool = new PerkPool(poolEntry.tag, corePerks);
                if (!RuntimePerkPools.ContainsKey(poolEntry.tag))
                {
                    RuntimePerkPools.Add(poolEntry.tag, pool);
                }
            }

            // 4. Build Rewards
            var coreRewards = rewards.Select(r => new LevelReward
            {
                Level = r.level,
                StatPoints = r.statPoints,
                FixedPerkIds = r.perkChoices.Where(p => p != null).Select(p => p.Id).ToList(),
                PerkPoolTag = r.perkPoolTag,
                PerkPoolDrawCount = r.perkPoolDrawCount
            }).ToList();

            _logic = new ProgressionLogic(thresholds, coreRewards);
        }

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
