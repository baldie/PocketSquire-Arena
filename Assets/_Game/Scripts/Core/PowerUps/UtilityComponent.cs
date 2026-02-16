#nullable enable
using System;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Power-up component that provides utility effects (e.g., healing after battle).
    /// </summary>
    [Serializable]
    public class UtilityComponent : PowerUpComponent
    {
        public enum UtilityType
        {
            PartialHeal
        }

        public UtilityType UtilityEffect { get; private set; }

        public override string UniqueKey => $"UTIL_{UtilityEffect.ToString().ToUpper()}";

        public override string IconId => UtilityEffect switch
        {
            UtilityType.PartialHeal => "heal",
            _ => "heal" // fallback
        };

        public override string DisplayName => $"{GetEffectName()} {RomanNumeral(Rank)}";

        public override string Description => 
            $"Restores {ComputeValue(1):F0}% of max health after each battle (scales with arena level).";

        public UtilityComponent(
            UtilityType utilityEffect, 
            float baseValue, 
            Rarity rarity, 
            PowerUpRank rank)
            : base(PowerUpComponentType.Utility, baseValue, rarity, rank)
        {
            UtilityEffect = utilityEffect;
        }

        /// <summary>
        /// Applies the utility effect to the player.
        /// </summary>
        public void ApplyToPlayer(Player player, int arenaLevel)
        {
            if (UtilityEffect == UtilityType.PartialHeal)
            {
                float healPercent = ComputeValue(arenaLevel) / 100f;
                int healAmount = (int)Math.Round(player.MaxHealth * healPercent);
                player.Heal(healAmount);
            }
        }

        private string GetEffectName()
        {
            return UtilityEffect switch
            {
                UtilityType.PartialHeal => "Battle Heal",
                _ => "Unknown"
            };
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
