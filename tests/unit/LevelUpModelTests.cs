using NUnit.Framework;
using PocketSquire.Arena.Core.LevelUp;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.Tests.LevelUp
{
    [TestFixture]
    public class LevelUpModelTests
    {
        private LevelUpModel _model;
        private Dictionary<string, int> _initialAttributes;
        private int _initialPoints;
        private int _currentLevel;

        [SetUp]
        public void Setup()
        {
            _initialAttributes = new Dictionary<string, int>
            {
                { "STR", 5 },
                { "CON", 10 },
                { "DEF", 3 }
            };
            _initialPoints = 3;
            _currentLevel = 5;
            _model = new LevelUpModel(_initialAttributes, _initialPoints, _currentLevel);
        }

        [Test]
        public void Initialization_ShouldSetCorrectValues()
        {
            Assert.That(_model.AvailablePoints, Is.EqualTo(3));
            Assert.That(_model.GetAttributeValue("STR"), Is.EqualTo(5));
            Assert.That(_model.GetAttributeValue("CON"), Is.EqualTo(10));
            Assert.That(_model.GetAttributeValue("DEF"), Is.EqualTo(3));
            Assert.That(_model.CurrentLevel, Is.EqualTo(5));
        }

        [Test]
        public void IncrementAttribute_ShouldDecreaseAvailablePoints_AndIncreaseAttribute()
        {
            _model.IncrementAttribute("STR");

            Assert.That(_model.AvailablePoints, Is.EqualTo(2));
            Assert.That(_model.GetAttributeValue("STR"), Is.EqualTo(6));
        }

        [Test]
        public void GetEligiblePerks_ShouldFilterByLevel()
        {
            var lowLevelPerk = new Perk("p1", "Low", "Desc", 1, null);
            var highLevelPerk = new Perk("p2", "High", "Desc", 10, null);

            var result = _model.GetEligiblePerks(new List<Perk> { lowLevelPerk, highLevelPerk });

            Assert.That(result, Contains.Item(lowLevelPerk));
            Assert.That(result, Does.Not.Contain(highLevelPerk));
        }

        [Test]
        public void GetEligiblePerks_ShouldFilterAlreadyUnlocked()
        {
            var perk = new Perk("p1", "Unlocked", "Desc", 1, null);
            _model.UnlockPerk("p1");

            var result = _model.GetEligiblePerks(new List<Perk> { perk });

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetEligiblePerks_ShouldFilterByPrerequisites()
        {
            var prereq = new Perk("prereq", "Prereq", "Desc", 1, null);
            var lockedPerk = new Perk("locked", "Locked", "Desc", 1, new List<string> { "prereq" });

            // Case 1: Prerequisite NOT unlocked
            var result = _model.GetEligiblePerks(new List<Perk> { lockedPerk });
            Assert.That(result, Is.Empty);

            // Case 2: Prerequisite Unlocked
            _model.UnlockPerk("prereq");
            result = _model.GetEligiblePerks(new List<Perk> { lockedPerk });
            Assert.That(result, Contains.Item(lockedPerk));
        }

        [Test]
        public void SelectPerk_ShouldUnlockPerk_IfPending()
        {
            var perkId = "choice1";
            _model.SetPendingPerkChoices(new List<string> { perkId, "other" });

            _model.SelectPerk(perkId);

            Assert.That(_model.IsPerkUnlocked(perkId), Is.True);
            Assert.That(_model.PendingPerkChoices, Is.Empty);
        }

        [Test]
        public void SelectPerk_ShouldNotUnlock_IfNotPending()
        {
            var perkId = "cheat";
            _model.SetPendingPerkChoices(new List<string> { "valid" });

            _model.SelectPerk(perkId);

            Assert.That(_model.IsPerkUnlocked(perkId), Is.False);
            Assert.That(_model.PendingPerkChoices, Is.Not.Empty);
        }

        [Test]
        public void IncrementDefenseAttribute_ShouldDecreaseAvailablePoints_AndIncreaseDefense()
        {
            _model.IncrementAttribute("DEF");

            Assert.That(_model.AvailablePoints, Is.EqualTo(2));
            Assert.That(_model.GetAttributeValue("DEF"), Is.EqualTo(4));
        }

        [Test]
        public void DecrementDefenseAttribute_ShouldIncreaseAvailablePoints_AndDecreaseDefense()
        {
            _model.IncrementAttribute("DEF");
            _model.DecrementAttribute("DEF");

            Assert.That(_model.AvailablePoints, Is.EqualTo(3));
            Assert.That(_model.GetAttributeValue("DEF"), Is.EqualTo(3));
        }
    }
}
