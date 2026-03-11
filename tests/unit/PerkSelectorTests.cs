
using NUnit.Framework;
using PocketSquire.Arena.Core.LevelUp;
using PocketSquire.Arena.Core.Perks;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Core.Tests.LevelUp
{
    [TestFixture]
    public class PerkSelectorTests
    {
        private static List<PlayerClass.ClassName> NoClasses() => new List<PlayerClass.ClassName>();

        [Test]
        public void Select_ShouldExcludeAlreadyUnlocked()
        {
            var p1 = new Perk { Id = "p1", DisplayName = "One", Prerequisites = new PerkPrerequisites { MinLevel = 1 } };
            var p2 = new Perk { Id = "p2", DisplayName = "Two", Prerequisites = new PerkPrerequisites { MinLevel = 1 } };
            var pool = new PerkPool("Test", new List<Perk> { p1, p2 });
            var context = new PerkSelector.SelectionContext { PlayerLevel = 5, UnlockedPerkIds = new HashSet<string> { "p1" } };
            // P1 is unlocked, so only P2 is eligible.

            var selected = PerkSelector.Select(pool, 2, context, new System.Random(123));

            Assert.That(selected, Has.Count.EqualTo(1));
            Assert.That(selected[0].Id, Is.EqualTo("p2"));
        }

        [Test]
        public void Select_ShouldExcludeBelowMinLevel()
        {
            var pLow = new Perk { Id = "low", DisplayName = "Low", Prerequisites = new PerkPrerequisites { MinLevel = 1 } };
            var pHigh = new Perk { Id = "high", DisplayName = "High", Prerequisites = new PerkPrerequisites { MinLevel = 10 } };
            var pool = new PerkPool("Test", new List<Perk> { pLow, pHigh });
            var context = new PerkSelector.SelectionContext { PlayerLevel = 5 }; // Level 5 < 10

            var selected = PerkSelector.Select(pool, 2, context, new System.Random(123));

            // Only low level perk should be returned
            Assert.That(selected, Has.Count.EqualTo(1));
            Assert.That(selected[0].Id, Is.EqualTo("low"));
        }

        [Test]
        public void Select_ShouldRespectPrerequisites()
        {
            var prereq = new Perk { Id = "prereq", DisplayName = "Prereq", Prerequisites = new PerkPrerequisites { MinLevel = 1 } };
            var chained = new Perk { Id = "chained", DisplayName = "Chained", Prerequisites = new PerkPrerequisites { MinLevel = 1, RequiredPerks = new List<string> { "prereq" } } };
            var pool = new PerkPool("Test", new List<Perk> { prereq, chained });
            var context = new PerkSelector.SelectionContext { PlayerLevel = 5, UnlockedPerkIds = new HashSet<string>() };

            // Neither unlocked. Prereq is eligible. Chained is NOT (prereq missing).
            var selectedNoUnlock = PerkSelector.Select(pool, 2, context, new System.Random(123));
            Assert.That(selectedNoUnlock, Has.Count.EqualTo(1));
            Assert.That(selectedNoUnlock[0].Id, Is.EqualTo("prereq"));

            // Now unlock prereq
            context.UnlockedPerkIds.Add("prereq");
            // Prereq excluded (already unlocked). Chained IS eligible (prereq met).
            var selectedWithUnlock = PerkSelector.Select(pool, 2, context, new System.Random(123));
            Assert.That(selectedWithUnlock, Has.Count.EqualTo(1));
            Assert.That(selectedWithUnlock[0].Id, Is.EqualTo("chained"));
        }

        [Test]
        public void Select_ShouldRespectCountLimit()
        {
            var p1 = new Perk { Id = "p1", DisplayName = "1", Prerequisites = new PerkPrerequisites { MinLevel = 1 } };
            var p2 = new Perk { Id = "p2", DisplayName = "2", Prerequisites = new PerkPrerequisites { MinLevel = 1 } };
            var p3 = new Perk { Id = "p3", DisplayName = "3", Prerequisites = new PerkPrerequisites { MinLevel = 1 } };
            var pool = new PerkPool("Test", new List<Perk> { p1, p2, p3 });
            var context = new PerkSelector.SelectionContext { PlayerLevel = 5 };

            var selected = PerkSelector.Select(pool, 2, context, new System.Random(123)); // Ask for 2

            Assert.That(selected, Has.Count.EqualTo(2));
        }
    }
}
