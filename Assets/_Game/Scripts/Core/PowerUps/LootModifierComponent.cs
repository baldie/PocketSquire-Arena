#nullable enable
using System;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Power-up component that modifies loot rewards (gold or XP).
    /// </summary>
    [Serializable]
    public class LootModifierComponent : PowerUpComponent
    {
        public enum LootType
        {
            Gold,
            Experience
        }

        public LootType TargetLoot { get; private set; }
        
        /// <summary>
        /// If true, this is a flat bonus (e.g., +1 coin fallback). Otherwise, it's a percentage.
        /// </summary>
        public bool IsFlatBonus { get; private set; }

        public override string UniqueKey => IsFlatBonus ? "COIN_FALLBACK" : $"LOOT_{TargetLoot.ToString().ToUpper()}";

        public override string IconId => TargetLoot switch
        {
            LootType.Gold => "gold",
            LootType.Experience => "xp",
            _ => "gold" // fallback
        };

        public override string DisplayName => 
            IsFlatBonus ? "Single Coin" : $"{TargetLoot} Bonus {RomanNumeral(Rank)}";

        public override string Description =>
            IsFlatBonus 
                ? "Grants +1 gold." 
                : $"Increases {TargetLoot} rewards by {ComputeValue(1):F0}% (scales with arena level).";

        public LootModifierComponent(
            LootType targetLoot, 
            float baseValue, 
            Rarity rarity, 
            PowerUpRank rank,
            bool isFlatBonus = false)
            : base(PowerUpComponentType.LootModifier, baseValue, rarity, rank)
        {
            TargetLoot = targetLoot;
            IsFlatBonus = isFlatBonus;
        }

        /// <summary>
        /// Returns the bonus percentage (or flat value if IsFlatBonus).
        /// </summary>
        public float GetBonusValue(int arenaLevel)
        {
            return IsFlatBonus ? BaseValue : ComputeValue(arenaLevel);
        }

        private static string RomanNumeral(PowerUpRank rank)
        {
            return rank switch
            {
                PowerUpRank.I => "I",
                PowerUpRank.II => "II",
                PowerUpRank.III => "III",
                _ => ""
            };
        }
    }
}
