using NUnit.Framework;
using PocketSquire.Arena.Core.PowerUps;
using System;

namespace PocketSquire.Arena.Core.Tests.PowerUps
{
    [TestFixture]
    public class PowerUpScalingTests
    {
        [Test]
        public void ComputeValue_Level1_CommonRankI_ReturnsBaseValue()
        {
            float result = PowerUpScaling.ComputeValue(10f, Rarity.Common, PowerUpRank.I, 1);
            
            // At level 1: (10 × 1.0 × 1.0) × (1 + ln(2)) ≈ 10 × 1.693 ≈ 16.93
            Assert.That(result, Is.EqualTo(10f * (1f + (float)Math.Log(2))).Within(0.01f));
        }

        [Test]
        public void ComputeValue_HigherArenaLevel_GrowsSubLinearly()
        {
            float level1 = PowerUpScaling.ComputeValue(10f, Rarity.Common, PowerUpRank.I, 1);
            float level10 = PowerUpScaling.ComputeValue(10f, Rarity.Common, PowerUpRank.I, 10);
            
            // Verify sub-linear growth: level 10 should be less than 2.1× level 1
            // (At level 10, ln(11) ≈ 2.398, so ratio is ~2.007)
            Assert.That(level10 / level1, Is.LessThan(2.1f));
            Assert.That(level10, Is.GreaterThan(level1));
        }

        [Test]
        public void ComputeValue_RarityAndRank_Multiply()
        {
            float commonRank1 = PowerUpScaling.ComputeValue(10f, Rarity.Common, PowerUpRank.I, 1);
            float legendaryRank3 = PowerUpScaling.ComputeValue(10f, Rarity.Legendary, PowerUpRank.III, 1);
            
            // Legendary (3.0×) × Rank III (2.0×) = 6× multiplier
            float expectedRatio = 3.0f * 2.0f;
            Assert.That(legendaryRank3 / commonRank1, Is.EqualTo(expectedRatio).Within(0.01f));
        }

        [Test]
        public void RollRarity_ZeroLuck_MostlyCommon()
        {
            var rng = new Random(42);
            int commonCount = 0;
            int trials = 1000;

            for (int i = 0; i < trials; i++)
            {
                var rarity = PowerUpScaling.RollRarity(0, rng);
                if (rarity == Rarity.Common) commonCount++;
            }

            // With 0 luck, ~70% should be common (allow ±10% variance)
            float commonPercent = (float)commonCount / trials;
            Assert.That(commonPercent, Is.GreaterThan(0.60f));
            Assert.That(commonPercent, Is.LessThan(0.80f));
        }

        [Test]
        public void RollRarity_HighLuck_ShiftsDistribution()
        {
            var rng = new Random(42);
            int commonCountLow = 0;
            int commonCountHigh = 0;
            int trials = 1000;

            // Low luck
            for (int i = 0; i < trials; i++)
            {
                var rarity = PowerUpScaling.RollRarity(0, new Random(42 + i));
                if (rarity == Rarity.Common) commonCountLow++;
            }

            // High luck
            for (int i = 0; i < trials; i++)
            {
                var rarity = PowerUpScaling.RollRarity(50, new Random(42 + i));
                if (rarity == Rarity.Common) commonCountHigh++;
            }

            // High luck should reduce common percentage
            Assert.That(commonCountHigh, Is.LessThan(commonCountLow));
        }
    }
}
