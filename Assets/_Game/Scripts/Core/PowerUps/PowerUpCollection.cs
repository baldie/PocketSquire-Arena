#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Tracks all power-ups owned by the player during a run.
    /// </summary>
    [Serializable]
    public class PowerUpCollection
    {
        private Dictionary<string, PowerUp> _powerUps = new();

        /// <summary>
        /// Adds a power-up to the collection. If already owned, increments rank instead.
        /// </summary>
        public void Add(PowerUp powerUp)
        {
            if (powerUp == null) throw new ArgumentNullException(nameof(powerUp));

            if (_powerUps.ContainsKey(powerUp.UniqueKey))
            {
                _powerUps[powerUp.UniqueKey].IncrementRank();
            }
            else
            {
                _powerUps[powerUp.UniqueKey] = powerUp;
            }
        }

        /// <summary>
        /// Returns true if the power-up with the given key is owned and at max rank.
        /// </summary>
        public bool HasAtMaxRank(string uniqueKey)
        {
            return _powerUps.ContainsKey(uniqueKey) && _powerUps[uniqueKey].IsMaxRank();
        }

        /// <summary>
        /// Returns true if the power-up with the given key is owned (at any rank).
        /// </summary>
        public bool Has(string uniqueKey)
        {
            return _powerUps.ContainsKey(uniqueKey);
        }

        /// <summary>
        /// Gets the current rank of a power-up, or null if not owned.
        /// </summary>
        public PowerUpRank? GetRank(string uniqueKey)
        {
            return _powerUps.ContainsKey(uniqueKey) ? _powerUps[uniqueKey].Rank : null;
        }

        /// <summary>
        /// Applies all monster debuffs to the given monster.
        /// </summary>
        public void ApplyMonsterDebuffs(Monster monster, int arenaLevel)
        {
            foreach (var powerUp in _powerUps.Values)
            {
                powerUp.Component.ApplyToMonster(monster, arenaLevel);
            }
        }

        /// <summary>
        /// Applies all utility effects (e.g., healing) to the player.
        /// </summary>
        public void ApplyUtilityEffects(Player player, int arenaLevel)
        {
            foreach (var powerUp in _powerUps.Values)
            {
                if (powerUp.Component is UtilityComponent utility)
                {
                    utility.ApplyToPlayer(player, arenaLevel);
                }
            }
        }

        /// <summary>
        /// Returns the total gold bonus percentage from all loot modifiers.
        /// </summary>
        public float GetGoldBonusPercent(int arenaLevel)
        {
            float total = 0f;
            foreach (var powerUp in _powerUps.Values)
            {
                if (powerUp.Component is LootModifierComponent loot && 
                    loot.TargetLoot == LootModifierComponent.LootType.Gold)
                {
                    total += loot.GetBonusValue(arenaLevel);
                }
            }
            return total;
        }

        /// <summary>
        /// Returns the total XP bonus percentage from all loot modifiers.
        /// </summary>
        public float GetXpBonusPercent(int arenaLevel)
        {
            float total = 0f;
            foreach (var powerUp in _powerUps.Values)
            {
                if (powerUp.Component is LootModifierComponent loot && 
                    loot.TargetLoot == LootModifierComponent.LootType.Experience)
                {
                    total += loot.GetBonusValue(arenaLevel);
                }
            }
            return total;
        }

        /// <summary>
        /// Returns all owned power-ups.
        /// </summary>
        public IEnumerable<PowerUp> GetAll() => _powerUps.Values;

        /// <summary>
        /// Returns the count of owned power-ups.
        /// </summary>
        public int Count => _powerUps.Count;
    }
}
