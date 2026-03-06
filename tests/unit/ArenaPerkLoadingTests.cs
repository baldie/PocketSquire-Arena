using NUnit.Framework;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Perks;
using System.IO;
using System;

namespace PocketSquire.Arena.Tests
{
    /// <summary>
    /// Tests for arena perk data loading and GameWorld helpers.
    /// </summary>
    [TestFixture]
    public class ArenaPerkLoadingTests
    {
        private static string GetProjectRoot()
        {
            string current = Environment.CurrentDirectory;
            while (!Directory.Exists(Path.Combine(current, "Assets")) && Directory.GetParent(current) != null)
            {
                var parent = Directory.GetParent(current);
                if (parent == null) break;
                current = parent.FullName;
            }
            return current;
        }

        [SetUp]
        public void Setup()
        {
            GameWorld.Load(GetProjectRoot());
        }

        [Test]
        public void LoadArenaPerks_Loads28Perks()
        {
            Assert.That(GameWorld.AllArenaPerks.Count, Is.EqualTo(28),
                "Expected 28 arena perks (metadata claims 30, trust the array)");
        }

        [Test]
        public void KeenEye_Passive_FieldsCorrect()
        {
            var perk = GameWorld.GetArenaPerkById("keen_eye");
            Assert.That(perk, Is.Not.Null);
            Assert.That(perk!.DisplayName, Is.EqualTo("Keen Eye"));
            Assert.That(perk.PerkType, Is.EqualTo(ArenaPerkType.Passive));
            Assert.That(perk.Vendor, Is.EqualTo(VendorType.Shopkeeper));
            Assert.That(perk.Cost, Is.EqualTo(100));
            Assert.That(perk.Price, Is.EqualTo(100)); // IMerchandise delegates to Cost
            Assert.That(perk.Tier, Is.EqualTo(0));
            Assert.That(perk.Prerequisites?.MinLevel, Is.EqualTo(1));
            Assert.That(perk.TriggerEvent, Is.Null, "Passive perk should have no event");
        }

        [Test]
        public void SecondWind_Triggered_FieldsCorrect()
        {
            var perk = GameWorld.GetArenaPerkById("second_wind");
            Assert.That(perk, Is.Not.Null);
            Assert.That(perk!.DisplayName, Is.EqualTo("Second Wind"));
            Assert.That(perk.PerkType, Is.EqualTo(ArenaPerkType.Triggered));
            Assert.That(perk.TriggerEvent, Is.EqualTo(PerkTriggerEvent.HPBelowThreshold));
            Assert.That(perk.Effect, Is.EqualTo(ArenaPerkEffectType.RestoreHP));
            Assert.That(perk.Value, Is.EqualTo(15));
            Assert.That(perk.IsPercent, Is.True);
            Assert.That(perk.OncePerBattle, Is.True);
            Assert.That(perk.Threshold, Is.EqualTo(30));
        }

        [Test]
        public void WarriorsResolve_ClassRestricted_FieldsCorrect()
        {
            var perk = GameWorld.GetArenaPerkById("warriors_resolve");
            Assert.That(perk, Is.Not.Null);
            Assert.That(perk!.Vendor, Is.EqualTo(VendorType.FightersBlacksmith));
            Assert.That(perk.Prerequisites?.ClassName, Is.EqualTo("Fighter"));
            Assert.That(perk.Prerequisites?.MinLevel, Is.EqualTo(5));
            Assert.That(perk.Effect, Is.EqualTo(ArenaPerkEffectType.StackDamageBuff));
            Assert.That(perk.MaxStacks, Is.EqualTo(5));
            Assert.That(perk.ResetOn, Is.EqualTo(PerkTriggerEvent.PlayerMissedMonster));
        }

        [Test]
        public void GetArenaPerkById_Unknown_ReturnsNull()
        {
            Assert.That(GameWorld.GetArenaPerkById("nonexistent_perk"), Is.Null);
        }

        [Test]
        public void GetArenaPerksByVendor_Shopkeeper_Returns6()
        {
            var shopkeeperPerks = GameWorld.GetArenaPerksByVendor(VendorType.Shopkeeper);
            Assert.That(shopkeeperPerks.Count, Is.EqualTo(6));
        }

        [Test]
        public void GetArenaPerksByVendor_Wizard_Returns8()
        {
            var wizardPerks = GameWorld.GetArenaPerksByVendor(VendorType.Wizard);
            Assert.That(wizardPerks.Count, Is.EqualTo(8));
        }

        [Test]
        public void GetArenaPerksByVendor_FightersBlacksmith_Returns8()
        {
            var perks = GameWorld.GetArenaPerksByVendor(VendorType.FightersBlacksmith);
            Assert.That(perks.Count, Is.EqualTo(8));
        }

        [Test]
        public void GetArenaPerksByVendor_ArcheryTrainer_Returns6()
        {
            var perks = GameWorld.GetArenaPerksByVendor(VendorType.ArcheryTrainer);
            Assert.That(perks.Count, Is.EqualTo(6));
        }

        // --- PlayerClass tier and slots ---

        [Test]
        public void GetTier_AllClasses_CorrectTiers()
        {
            Assert.That(PlayerClass.GetTier(PlayerClass.ClassName.Squire),       Is.EqualTo(0));
            Assert.That(PlayerClass.GetTier(PlayerClass.ClassName.Fighter),      Is.EqualTo(1));
            Assert.That(PlayerClass.GetTier(PlayerClass.ClassName.SpellCaster),  Is.EqualTo(1));
            Assert.That(PlayerClass.GetTier(PlayerClass.ClassName.Bowman),       Is.EqualTo(1));
            Assert.That(PlayerClass.GetTier(PlayerClass.ClassName.Mage),         Is.EqualTo(2));
            Assert.That(PlayerClass.GetTier(PlayerClass.ClassName.Warrior),      Is.EqualTo(2));
            Assert.That(PlayerClass.GetTier(PlayerClass.ClassName.Wizard),       Is.EqualTo(3));
            Assert.That(PlayerClass.GetTier(PlayerClass.ClassName.Knight),       Is.EqualTo(3));
            Assert.That(PlayerClass.GetTier(PlayerClass.ClassName.Sorcerer),     Is.EqualTo(4));
            Assert.That(PlayerClass.GetTier(PlayerClass.ClassName.Paladin),      Is.EqualTo(4));
        }

        [Test]
        public void GetMaxPerkSlots_AllTiers_Correct()
        {
            Assert.That(PlayerClass.GetMaxPerkSlots(PlayerClass.ClassName.Squire),   Is.EqualTo(2));
            Assert.That(PlayerClass.GetMaxPerkSlots(PlayerClass.ClassName.Fighter),  Is.EqualTo(4));
            Assert.That(PlayerClass.GetMaxPerkSlots(PlayerClass.ClassName.Mage),     Is.EqualTo(6));
            Assert.That(PlayerClass.GetMaxPerkSlots(PlayerClass.ClassName.Wizard),   Is.EqualTo(8));
            Assert.That(PlayerClass.GetMaxPerkSlots(PlayerClass.ClassName.Sorcerer), Is.EqualTo(10));
        }
    }
}
