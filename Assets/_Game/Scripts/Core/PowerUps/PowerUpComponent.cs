#nullable enable
using System;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Base class for all power-up components. Each component represents a specific effect.
    /// </summary>
    [Serializable]
    public abstract class PowerUpComponent
    {
        public PowerUpComponentType ComponentType { get; protected set; }
        public Rarity Rarity { get; set; }
        public PowerUpRank Rank { get; set; }
        public float BaseValue { get; protected set; }
        
        /// <summary>
        /// Unique identifier for this component type (e.g., "ATK_STR", "LOOT_GOLD").
        /// Used to detect duplicates and handle rank-ups.
        /// </summary>
        public abstract string UniqueKey { get; }
        
        /// <summary>
        /// Icon sprite ID for GameAssetRegistry lookup (e.g., "str", "gold", "heal").
        /// </summary>
        public abstract string IconId { get; }
        
        /// <summary>
        /// Human-readable name for UI display.
        /// </summary>
        public abstract string DisplayName { get; }
        
        /// <summary>
        /// Description of what this component does.
        /// </summary>
        public abstract string Description { get; }

        protected PowerUpComponent(PowerUpComponentType type, float baseValue, Rarity rarity, PowerUpRank rank)
        {
            ComponentType = type;
            BaseValue = baseValue;
            Rarity = rarity;
            Rank = rank;
        }

        /// <summary>
        /// Computes the scaled value for this component at the given arena level.
        /// </summary>
        public float ComputeValue(int arenaLevel)
        {
            return PowerUpScaling.ComputeValue(BaseValue, Rarity, Rank, arenaLevel);
        }

        /// <summary>
        /// Applies this component's effect to a monster (for debuffs). Default is no-op.
        /// </summary>
        public virtual void ApplyToMonster(Monster monster, int arenaLevel)
        {
            // Override in MonsterDebuffComponent
        }
    }
}
