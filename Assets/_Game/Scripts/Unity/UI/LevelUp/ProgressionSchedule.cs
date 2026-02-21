#nullable enable
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using PocketSquire.Arena.Core.LevelUp;
using PocketSquire.Arena.Core;

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
            // If empty, it's "Global". Otherwise, only these classes get this reward.
            public List<PlayerClass.ClassName> ValidClasses = new List<PlayerClass.ClassName>(); 
            public int statPoints;
            public List<PerkNode> perkChoices = new List<PerkNode>();
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

        public LevelReward GetRewardForLevel(int level, PlayerClass.ClassName currentClass)
        {
            // 1. Find the entry that matches the level AND the class
            // We look for entries specifically for this class, or entries with no class restrictions
            var entry = rewards.FirstOrDefault(r => r.level == level && 
                        (r.ValidClasses.Count == 0 || r.ValidClasses.Contains(currentClass)));

            if (entry == null) return new LevelReward();

            return new LevelReward
            {
                Level = entry.level,
                StatPoints = entry.statPoints,
                FixedPerkIds = entry.perkChoices.Where(p => p != null).Select(p => p.Id).ToList(),
                PerkPoolTag = entry.perkPoolTag,
                PerkPoolDrawCount = entry.perkPoolDrawCount
            };
        }

        public List<Perk> GetAvailablePerks(int level, PlayerClass.ClassName playerClass)
        {
            // 1. Find the reward entry for this level
            var reward = rewards.FirstOrDefault(r => r.level == level);
            if (reward == null) return new List<Perk>();
            
            // 2. Get the pool
            if (RuntimePerkPools.TryGetValue(reward.perkPoolTag, out var pool))
            {
                // 3. Filter perks inside the pool that are allowed for this class
                return pool.Perks
                    .Where(p => p.AllowedClasses.Count == 0 || p.AllowedClasses.Contains(playerClass))
                    .OrderBy(x => UnityEngine.Random.value) // Shuffle
                    .Take(reward.perkPoolDrawCount)
                    .ToList();
            }
            return new List<Perk>();
        }

        public List<Perk> DrawPerksForPlayer(string poolTag, int count, PlayerClass.ClassName playerClass, int playerLevel, List<string> ownedPerkIds)
        {
            if (!RuntimePerkPools.TryGetValue(poolTag, out var pool)) return new List<Perk>();

            return pool.Perks
                .Where(p => 
                    // 1. Is the player high enough level?
                    playerLevel >= p.MinLevel && 
                    // 2. Is this perk allowed for the player's class?
                    (p.AllowedClasses.Count == 0 || p.AllowedClasses.Contains(playerClass)) &&
                    // 3. Does the player already have it? (Assuming non-stackable)
                    !ownedPerkIds.Contains(p.Id) &&
                    // 4. Have prerequisites been met?
                    p.PrerequisitePerkIds.All(id => ownedPerkIds.Contains(id))
                )
                .OrderBy(_ => UnityEngine.Random.value) // Shuffle
                .Take(count)
                .ToList();
        }

        public List<Perk> GetCommonPerkChoices(string poolTag, int count)
        {
            if (!RuntimePerkPools.TryGetValue(poolTag, out var pool)) 
                return new List<Perk>();

            return pool.Perks
                .Where(p => p.AllowedClasses.Count == 0) // ONLY Common perks
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(count)
                .ToList();
        }

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
