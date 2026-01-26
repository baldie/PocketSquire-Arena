#nullable enable
using System;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.LevelUp
{
    public class LevelUpModel : ILevelUpModel
    {
        private Dictionary<string, int> _startingAttributes;
        private Dictionary<string, int> _currentAttributes;
        private int _startingAvailablePoints;
        private int _currentAvailablePoints;
        private HashSet<string> _unlockedPerkIds;

        public int AvailablePoints => _currentAvailablePoints;
        public int CurrentLevel { get; private set; }
        public List<string> PendingPerkChoices { get; private set; }

        public event Action? OnStatsChanged;
        public event Action? OnPerksChanged;

        public LevelUpModel(Dictionary<string, int> currentAttributes, int availablePoints, int currentLevel, IEnumerable<string>? unlockedPerks = null)
        {
            _startingAttributes = new Dictionary<string, int>(currentAttributes);
            _currentAttributes = new Dictionary<string, int>(currentAttributes);
            _startingAvailablePoints = availablePoints;
            _currentAvailablePoints = availablePoints;
            CurrentLevel = currentLevel;
            _unlockedPerkIds = unlockedPerks != null ? new HashSet<string>(unlockedPerks!) : new HashSet<string>();
            PendingPerkChoices = new List<string>();
        }

        public int GetAttributeValue(string attributeKey)
        {
            if (_currentAttributes.ContainsKey(attributeKey))
            {
                return _currentAttributes[attributeKey];
            }
            return 0;
        }

        public int GetStartingAttributeValue(string attributeKey)
        {
            if (_startingAttributes.ContainsKey(attributeKey))
            {
                return _startingAttributes[attributeKey];
            }
            return 0;
        }

        public void IncrementAttribute(string attributeKey)
        {
            if (_currentAvailablePoints > 0 && _currentAttributes.ContainsKey(attributeKey))
            {
                _currentAttributes[attributeKey]++;
                _currentAvailablePoints--;
                OnStatsChanged?.Invoke();
            }
        }

        public void DecrementAttribute(string attributeKey)
        {
            if (_currentAttributes.ContainsKey(attributeKey))
            {
                // Cannot go below starting value
                if (_currentAttributes[attributeKey] > _startingAttributes[attributeKey])
                {
                    _currentAttributes[attributeKey]--;
                    _currentAvailablePoints++;
                    OnStatsChanged?.Invoke();
                }
            }
        }

        public List<Perk> GetEligiblePerks(List<Perk> pool)
        {
            var eligible = new List<Perk>();
            if (pool == null) return eligible;

            foreach (var perk in pool)
            {
                if (_unlockedPerkIds.Contains(perk.Id)) continue;
                if (perk.MinLevel > CurrentLevel) continue;

                bool prereqsMet = true;
                foreach (var prereqId in perk.PrerequisitePerkIds)
                {
                    if (!_unlockedPerkIds.Contains(prereqId))
                    {
                        prereqsMet = false;
                        break;
                    }
                }

                if (prereqsMet)
                {
                    eligible.Add(perk);
                }
            }
            return eligible;
        }

        public void SelectPerk(string perkId)
        {
            if (PendingPerkChoices.Contains(perkId))
            {
                UnlockPerk(perkId);
                PendingPerkChoices.Clear();
                OnPerksChanged?.Invoke();
            }
        }

        public bool IsPerkUnlocked(string perkId)
        {
            return _unlockedPerkIds.Contains(perkId);
        }

        public void UnlockPerk(string perkId)
        {
            if (!_unlockedPerkIds.Contains(perkId))
            {
                _unlockedPerkIds.Add(perkId);
                OnPerksChanged?.Invoke();
            }
        }

        public void SetPendingPerkChoices(List<string> perkIds)
        {
            PendingPerkChoices = perkIds ?? new List<string>();
            OnPerksChanged?.Invoke();
        }
    }
}
