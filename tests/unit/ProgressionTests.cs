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
            var config = new List<LevelReward>
            {
                new LevelReward { Level = 1, ExperienceRequired = 0 },
                new LevelReward { Level = 2, ExperienceRequired = 100 },
                new LevelReward { Level = 3, ExperienceRequired = 50 } // Error: lower than level 2
            };

            var logic = new ProgressionLogic(config);
            bool isValid = logic.IsValid(out string error);

            Assert.That(isValid, Is.False);
            Assert.That(error, Contains.Substring("Level 3 XP requirement (50) must be higher than Level 2 (100)"));
        }

        [Test]
        public void IsValid_ShouldReturnTrue_WhenRequirementsAreMonotonic()
        {
            var config = new List<LevelReward>
            {
                new LevelReward { Level = 1, ExperienceRequired = 0 },
                new LevelReward { Level = 2, ExperienceRequired = 100 },
                new LevelReward { Level = 3, ExperienceRequired = 250 }
            };

            var logic = new ProgressionLogic(config);
            Assert.That(logic.IsValid(out _), Is.True);
        }

        [Test]
        public void GetLevelForExperience_ShouldReturnCorrectLevel()
        {
            var config = new List<LevelReward>
            {
                new LevelReward { Level = 1, ExperienceRequired = 0 },
                new LevelReward { Level = 2, ExperienceRequired = 100 },
                new LevelReward { Level = 3, ExperienceRequired = 300 }
            };

            var logic = new ProgressionLogic(config);

            Assert.That(logic.GetLevelForExperience(0), Is.EqualTo(1));
            Assert.That(logic.GetLevelForExperience(50), Is.EqualTo(1));
            Assert.That(logic.GetLevelForExperience(100), Is.EqualTo(2));
            Assert.That(logic.GetLevelForExperience(299), Is.EqualTo(2));
            Assert.That(logic.GetLevelForExperience(300), Is.EqualTo(3));
            Assert.That(logic.GetLevelForExperience(5000), Is.EqualTo(3));
        }

        [Test]
        public void GetLevelForExperience_ShouldHandleUnorderedInput()
        {
            var config = new List<LevelReward>
            {
                new LevelReward { Level = 3, ExperienceRequired = 300 },
                new LevelReward { Level = 1, ExperienceRequired = 0 },
                new LevelReward { Level = 2, ExperienceRequired = 100 }
            };

            var logic = new ProgressionLogic(config);

            Assert.That(logic.GetLevelForExperience(150), Is.EqualTo(2));
        }
    }
}
