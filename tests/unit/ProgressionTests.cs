using NUnit.Framework;
using PocketSquire.Arena.Core.LevelUp;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.Tests.LevelUp
{
    [TestFixture]
    public class ProgressionTests
    {
        [Test]
        public void IsValid_ShouldReturnFalse_WhenNextLevelXPisLowerThanCurrent()
        {
            // Level 1 = 0, Level 2 = 100, Level 3 = 50 (Invalid)
            var thresholds = new int[] { 0, 100, 50 };
            var rewards = new List<LevelReward>
            {
                new LevelReward { Level = 1 },
                new LevelReward { Level = 2 },
                new LevelReward { Level = 3 }
            };

            var logic = new ProgressionLogic(thresholds, rewards);
            bool isValid = logic.IsValid(out string error);

            Assert.That(isValid, Is.False);
            // Error message format: "Level {i+1} XP ({curr}) is higher than Level {i+2} ({next})."
            // Level 2 (100) > Level 3 (50)
            Assert.That(error, Contains.Substring("Level 2 XP (100) is higher than Level 3 (50)"));
        }

        [Test]
        public void IsValid_ShouldReturnTrue_WhenRequirementsAreMonotonic()
        {
            var thresholds = new int[] { 0, 100, 250 };
            var rewards = new List<LevelReward>
            {
                new LevelReward { Level = 1 },
                new LevelReward { Level = 2 },
                new LevelReward { Level = 3 }
            };

            var logic = new ProgressionLogic(thresholds, rewards);
            Assert.That(logic.IsValid(out _), Is.True);
        }

        [Test]
        public void GetLevelForExperience_ShouldReturnCorrectLevel()
        {
            var thresholds = new int[] { 0, 100, 300 }; // Lvl 1=0, Lvl 2=100, Lvl 3=300
            var rewards = new List<LevelReward>
            {
                new LevelReward { Level = 1 },
                new LevelReward { Level = 2 },
                new LevelReward { Level = 3 }
            };

            var logic = new ProgressionLogic(thresholds, rewards);

            Assert.That(logic.GetLevelForExperience(0), Is.EqualTo(1));
            Assert.That(logic.GetLevelForExperience(50), Is.EqualTo(1));
            Assert.That(logic.GetLevelForExperience(100), Is.EqualTo(2));
            Assert.That(logic.GetLevelForExperience(299), Is.EqualTo(2));
            Assert.That(logic.GetLevelForExperience(300), Is.EqualTo(3));
            Assert.That(logic.GetLevelForExperience(5000), Is.EqualTo(3)); // Cap at max level
        }

        [Test]
        public void GetXpToNextLevel_ShouldReturnCorrectDelta()
        {
            var thresholds = new int[] { 0, 100, 300 };
            var rewards = new List<LevelReward>(); // rewards strictly don't matter for XP calculation
            
            var logic = new ProgressionLogic(thresholds, rewards);

            // Current XP = 50 (Level 1). Next Level = 2 (100 XP). Delta = 50.
            Assert.That(logic.GetXpToNextLevel(50), Is.EqualTo(50));
            
            // Current XP = 100 (Level 2). Next Level = 3 (300 XP). Delta = 200.
            Assert.That(logic.GetXpToNextLevel(100), Is.EqualTo(200));
        }
    }
}
