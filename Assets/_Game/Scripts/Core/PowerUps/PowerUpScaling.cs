using System;

namespace PocketSquire.Arena.Core.PowerUps
{
    /// <summary>
    /// Static helper for power-up value scaling with diminishing returns.
    /// Formula: Value = (BaseValue × RarityMult × RankMult) × (1 + ln(ArenaLevel + 1))
    /// </summary>
    public static class PowerUpScaling
    {
        /// <summary>
        /// Computes the scaled value for a power-up component.
        /// </summary>
        public static float ComputeValue(float baseValue, Rarity rarity, PowerUpRank rank, int arenaLevel)
        {
            float rarityMult = GetRarityMultiplier(rarity);
            float rankMult = GetRankMultiplier(rank);
            float levelFactor = 1f + (float)Math.Log(arenaLevel + 1);
            
            return baseValue * rarityMult * rankMult * levelFactor;
        }

        /// <summary>
        /// Rolls a rarity tier based on luck stat. Higher luck shifts distribution toward rarer tiers.
        /// </summary>
        public static Rarity RollRarity(int luckStat, Random rng)
        {
            // Base cumulative probabilities (luck = 0):
            // Common: 70%, Rare: 20%, Epic: 8%, Legendary: 2%
            float commonThreshold = 0.70f;
            float rareThreshold = 0.90f;
            float epicThreshold = 0.98f;
            
            // Luck shifts thresholds downward (makes rarer items more likely)
            // Each point of luck reduces common threshold by 0.5%, increases legendary chance
            float luckFactor = luckStat * 0.005f;
            commonThreshold = Math.Max(0.40f, commonThreshold - luckFactor);
            rareThreshold = Math.Max(commonThreshold + 0.10f, rareThreshold - luckFactor * 0.5f);
            epicThreshold = Math.Max(rareThreshold + 0.05f, epicThreshold - luckFactor * 0.25f);
            
            float roll = (float)rng.NextDouble();
            
            if (roll < commonThreshold) return Rarity.Common;
            if (roll < rareThreshold) return Rarity.Rare;
            if (roll < epicThreshold) return Rarity.Epic;
            return Rarity.Legendary;
        }

        private static float GetRarityMultiplier(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => 1.0f,
                Rarity.Rare => 1.5f,
                Rarity.Epic => 2.0f,
                Rarity.Legendary => 3.0f,
                _ => 1.0f
            };
        }

        private static float GetRankMultiplier(PowerUpRank rank)
        {
            return rank switch
            {
                PowerUpRank.I => 1.0f,
                PowerUpRank.II => 1.5f,
                PowerUpRank.III => 2.0f,
                _ => 1.0f
            };
        }
    }
}
