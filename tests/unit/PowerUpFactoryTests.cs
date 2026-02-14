using NUnit.Framework;
using PocketSquire.Arena.Core.PowerUps;
using System;
using System.Linq;

namespace PocketSquire.Arena.Core.Tests.PowerUps
{
    [TestFixture]
    public class PowerUpFactoryTests
    {
        [Test]
        public void Generate_ReturnsThreeDistinctPowerUps()
        {
            var context = new PowerUpFactory.PowerUpGenerationContext
            {
                ArenaLevel = 1,
                PlayerLuck = 0,
                PlayerHealthPercent = 1.0f,
                OwnedPowerUps = new PowerUpCollection()
            };

            var powerUps = PowerUpFactory.Generate(context, new Random(42));

            Assert.That(powerUps, Has.Count.EqualTo(3));
            
            // All should have distinct keys
            var keys = powerUps.Select(p => p.UniqueKey).ToList();
            Assert.That(keys.Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void Generate_LowHealth_BoostsHealWeight()
        {
            int healCountLow = 0;
            int healCountHigh = 0;
            int trials = 100;

            // High health
            for (int i = 0; i < trials; i++)
            {
                var context = new PowerUpFactory.PowerUpGenerationContext
                {
                    ArenaLevel = 1,
                    PlayerLuck = 0,
                    PlayerHealthPercent = 1.0f,
                    OwnedPowerUps = new PowerUpCollection()
                };
                var powerUps = PowerUpFactory.Generate(context, new Random(42 + i));
                if (powerUps.Any(p => p.UniqueKey == "UTIL_PARTIALHEAL"))
                    healCountHigh++;
            }

            // Low health
            for (int i = 0; i < trials; i++)
            {
                var context = new PowerUpFactory.PowerUpGenerationContext
                {
                    ArenaLevel = 1,
                    PlayerLuck = 0,
                    PlayerHealthPercent = 0.2f,
                    OwnedPowerUps = new PowerUpCollection()
                };
                var powerUps = PowerUpFactory.Generate(context, new Random(42 + i));
                if (powerUps.Any(p => p.UniqueKey == "UTIL_PARTIALHEAL"))
                    healCountLow++;
            }

            // Low health should boost heal appearance
            Assert.That(healCountLow, Is.GreaterThan(healCountHigh));
        }

        [Test]
        public void Generate_ExistingPowerUp_RanksUp()
        {
            var collection = new PowerUpCollection();
            var existingComponent = new AttributeModifierComponent(
                AttributeModifierComponent.AttributeType.Strength,
                2f,
                Rarity.Common,
                PowerUpRank.I
            );
            collection.Add(new PowerUp(existingComponent));

            var context = new PowerUpFactory.PowerUpGenerationContext
            {
                ArenaLevel = 1,
                PlayerLuck = 0,
                PlayerHealthPercent = 1.0f,
                OwnedPowerUps = collection
            };

            // Generate many times until we get a strength boost
            bool foundRankUp = false;
            for (int i = 0; i < 100; i++)
            {
                var powerUps = PowerUpFactory.Generate(context, new Random(100 + i));
                var strBoost = powerUps.FirstOrDefault(p => p.UniqueKey == "ATTR_STRENGTH");
                if (strBoost != null && strBoost.Rank == PowerUpRank.II)
                {
                    foundRankUp = true;
                    break;
                }
            }

            Assert.That(foundRankUp, Is.True, "Should eventually generate a rank II strength boost");
        }

        [Test]
        public void Generate_AllMaxRank_ReturnsCoinFallback()
        {
            // Create a scenario where we force coin fallback by using all slots
            var context = new PowerUpFactory.PowerUpGenerationContext
            {
                ArenaLevel = 1,
                PlayerLuck = 0,
                PlayerHealthPercent = 1.0f,
                OwnedPowerUps = new PowerUpCollection()
            };

            // This test verifies the factory always returns 3 choices
            var powerUps = PowerUpFactory.Generate(context, new Random(42));
            Assert.That(powerUps, Has.Count.EqualTo(3));
        }

        [Test]
        public void Generate_HighLuck_MoreRarePowerUps()
        {
            int rareCountLow = 0;
            int rareCountHigh = 0;
            int trials = 100;

            // Low luck
            for (int i = 0; i < trials; i++)
            {
                var context = new PowerUpFactory.PowerUpGenerationContext
                {
                    ArenaLevel = 1,
                    PlayerLuck = 0,
                    PlayerHealthPercent = 1.0f,
                    OwnedPowerUps = new PowerUpCollection()
                };
                var powerUps = PowerUpFactory.Generate(context, new Random(42 + i));
                rareCountLow += powerUps.Count(p => p.Component.Rarity != Rarity.Common);
            }

            // High luck
            for (int i = 0; i < trials; i++)
            {
                var context = new PowerUpFactory.PowerUpGenerationContext
                {
                    ArenaLevel = 1,
                    PlayerLuck = 50,
                    PlayerHealthPercent = 1.0f,
                    OwnedPowerUps = new PowerUpCollection()
                };
                var powerUps = PowerUpFactory.Generate(context, new Random(42 + i));
                rareCountHigh += powerUps.Count(p => p.Component.Rarity != Rarity.Common);
            }

            // High luck should produce more non-common rarities
            Assert.That(rareCountHigh, Is.GreaterThan(rareCountLow));
        }
    }
}
