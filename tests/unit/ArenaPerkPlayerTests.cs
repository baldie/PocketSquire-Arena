using NUnit.Framework;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Perks;
using Newtonsoft.Json;
using System.IO;
using System;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class ArenaPerkPlayerTests
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

        private Player MakeSquireWithGold(int gold = 500)
        {
            var p = new Player("TestHero", 20, 20, new Attributes(), Player.Genders.m);
            p.GainGold(gold);
            return p;
        }

        private ArenaPerk MakePerk(string id = "test_perk", int cost = 100)
        {
            return new ArenaPerk
            {
                Id = id,
                DisplayName = "Test Perk",
                Cost = cost,
                PerkType = ArenaPerkType.Passive,
                Vendor = VendorType.Shopkeeper
            };
        }

        // --- TryPurchaseArenaPerk ---

        [Test]
        public void TryPurchaseArenaPerk_Success_GoldDeductedAndIdAdded()
        {
            var player = MakeSquireWithGold(500);
            var perk = MakePerk(cost: 100);

            bool result = player.TryPurchaseArenaPerk(perk);

            Assert.That(result, Is.True);
            Assert.That(player.Gold, Is.EqualTo(400));
            Assert.That(player.AcquiredPerks, Contains.Item(perk.Id));
        }

        [Test]
        public void TryPurchaseArenaPerk_AlreadyOwned_ReturnsFalse()
        {
            var player = MakeSquireWithGold(500);
            var perk = MakePerk(cost: 100);
            player.TryPurchaseArenaPerk(perk);

            bool second = player.TryPurchaseArenaPerk(perk);

            Assert.That(second, Is.False);
            Assert.That(player.Gold, Is.EqualTo(400), "Gold should not be spent twice");
        }

        [Test]
        public void TryPurchaseArenaPerk_InsufficientGold_ReturnsFalse()
        {
            var player = MakeSquireWithGold(50);
            var perk = MakePerk(cost: 100);

            bool result = player.TryPurchaseArenaPerk(perk);

            Assert.That(result, Is.False);
            Assert.That(player.Gold, Is.EqualTo(50), "Gold unchanged on failure");
            Assert.That(player.AcquiredPerks, Does.Not.Contain(perk.Id));
        }

        [Test]
        public void TryPurchaseArenaPerk_NullPerk_ThrowsArgumentNull()
        {
            var player = MakeSquireWithGold();
            Assert.Throws<ArgumentNullException>(() => player.TryPurchaseArenaPerk(null!));
        }

        // --- CanActivateArenaPerk ---

        [Test]
        public void CanActivateArenaPerk_AllowsSwap_WhenInventoryBelowFutureCapacity()
        {
            var player = MakeSquireWithGold();
            var perkToRemove = "satchel_tier_1";
            var perkToActivate = MakePerk("satchel_tier_2"); // Going to a bigger bag

            player.Inventory.AddItem(1, 1);
            player.Inventory.AddItem(2, 1);

            bool canSwap = player.CanActivateArenaPerk(perkToRemove, perkToActivate);

            Assert.That(canSwap, Is.True);
        }

        [Test]
        public void CanActivateArenaPerk_BlocksSwap_WhenInventoryAboveFutureCapacity()
        {
            var player = MakeSquireWithGold();
            
            // Assume the player currently has tier 1 and tier 1 gives 3 slots.
            player.ActiveArenaPerkIds.Add("satchel_tier_1");
            player.Inventory.UpdateCapacity(player.GetActivePerks());
            
            // Fill 3 slots
            player.Inventory.AddItem(1, 1);
            player.Inventory.AddItem(2, 1);
            player.Inventory.AddItem(3, 1);

            // Removing tier 1 will drop max capacity to base (2).
            
            // For dropping capacity on removal, PerkUI handles the explicit check.
            // But if we test CanActivateArenaPerk, we can simulate swapping from tier 1 to a non-bag perk.
            var perkToActivate = MakePerk("combat_perk_1");
            bool canSwap = player.CanActivateArenaPerk("satchel_tier_1", perkToActivate);

            Assert.That(canSwap, Is.False, "Should prevent removing bag when inventory has 3 slots filled.");
        }

        [Test]
        public void CanActivateArenaPerk_AllowsSwap_SmallerToLarger_WhenCurrentAreFull()
        {
            var player = MakeSquireWithGold();
            
            // Assume the player currently has tier 1 and tier 1 gives 3 slots.
            player.ActiveArenaPerkIds.Add("satchel_tier_1");
            player.Inventory.UpdateCapacity(player.GetActivePerks());
            
            // Fill 3 slots
            player.Inventory.AddItem(1, 1);
            player.Inventory.AddItem(2, 1);
            player.Inventory.AddItem(3, 1);

            // Upgrading from tier 1 (3 slots) to tier 2 (4 slots).
            var perkToActivate = MakePerk("satchel_tier_2");
            bool canSwap = player.CanActivateArenaPerk("satchel_tier_1", perkToActivate);

            Assert.That(canSwap, Is.True, "Should allow upgrading bag from 3 to 4 slots even when 3 are filled.");
        }

        [Test]
        public void CanActivateArenaPerk_BlocksMultipleActiveSatchels_WhenAddingNewSatchel()
        {
            var player = MakeSquireWithGold();
            
            player.ActiveArenaPerkIds.Add("satchel_tier_1");
            player.Inventory.UpdateCapacity(player.GetActivePerks());

            var perkToActivate = MakePerk("satchel_tier_2");
            bool canSwap = player.CanActivateArenaPerk(null, perkToActivate);

            Assert.That(canSwap, Is.False, "Should prevent having both tier 1 and tier 2 satchels active at the same time.");
        }

        [Test]
        public void CanActivateArenaPerk_AllowsSwap_WhenReplacingSatchelWithAnotherSatchel()
        {
            var player = MakeSquireWithGold();
            
            player.ActiveArenaPerkIds.Add("satchel_tier_1");
            player.Inventory.UpdateCapacity(player.GetActivePerks());

            // Swapping out the existing satchel for another one (which is also an upgrade so capacity is fine).
            var perkToActivate = MakePerk("satchel_tier_2");
            bool canSwap = player.CanActivateArenaPerk("satchel_tier_1", perkToActivate);

            Assert.That(canSwap, Is.True, "Should allow swapping a satchel for another satchel.");
        }

        // --- TryActivateArenaPerk ---

        [Test]
        public void TryActivateArenaPerk_Success_AddsToActiveAndCreatesState()
        {
            var player = MakeSquireWithGold();
            var perk = MakePerk();
            player.TryPurchaseArenaPerk(perk);

            bool result = player.TryActivateArenaPerk(perk.Id);

            Assert.That(result, Is.True);
            Assert.That(player.ActiveArenaPerkIds, Contains.Item(perk.Id));
            Assert.That(player.ArenaPerkStates.ContainsKey(perk.Id), Is.True);
        }

        [Test]
        public void TryActivateArenaPerk_NotOwned_ReturnsFalse()
        {
            var player = MakeSquireWithGold();

            bool result = player.TryActivateArenaPerk("not_owned_perk");

            Assert.That(result, Is.False);
        }

        [Test]
        public void TryActivateArenaPerk_Squire_CapIs2_ThirdActivationFails()
        {
            var player = MakeSquireWithGold(10000);
            Assert.That(player.MaxArenaPerkSlots, Is.EqualTo(2), "Squire should have 2 slots");

            var p1 = MakePerk("perk_1"); var p2 = MakePerk("perk_2"); var p3 = MakePerk("perk_3");
            player.TryPurchaseArenaPerk(p1);
            player.TryPurchaseArenaPerk(p2);
            player.TryPurchaseArenaPerk(p3);

            player.TryActivateArenaPerk(p1.Id);
            player.TryActivateArenaPerk(p2.Id);
            bool third = player.TryActivateArenaPerk(p3.Id);

            Assert.That(third, Is.False, "Should not activate beyond cap");
        }

        [Test]
        public void TryActivateArenaPerk_AlreadyActive_ReturnsFalse()
        {
            var player = MakeSquireWithGold();
            var perk = MakePerk();
            player.TryPurchaseArenaPerk(perk);
            player.TryActivateArenaPerk(perk.Id);

            bool second = player.TryActivateArenaPerk(perk.Id);

            Assert.That(second, Is.False);
        }

        [Test]
        public void MaxArenaPerkSlots_ChangesWithClass()
        {
            var player = MakeSquireWithGold();
            Assert.That(player.MaxArenaPerkSlots, Is.EqualTo(2)); // Squire = tier 0 = 2 slots

            // Simulate class change — directly test GetMaxPerkSlots
            Assert.That(PlayerClass.GetMaxPerkSlots(PlayerClass.ClassName.Fighter), Is.EqualTo(4));
            Assert.That(PlayerClass.GetMaxPerkSlots(PlayerClass.ClassName.Mage), Is.EqualTo(6));
        }

        // --- TryDeactivateArenaPerk ---

        [Test]
        public void TryDeactivateArenaPerk_Success_RemovesFromActiveAndState()
        {
            var player = MakeSquireWithGold();
            var perk = MakePerk();
            player.TryPurchaseArenaPerk(perk);
            player.TryActivateArenaPerk(perk.Id);

            bool result = player.TryDeactivateArenaPerk(perk.Id);

            Assert.That(result, Is.True);
            Assert.That(player.ActiveArenaPerkIds, Does.Not.Contain(perk.Id));
            Assert.That(player.ArenaPerkStates.ContainsKey(perk.Id), Is.False);
        }

        [Test]
        public void TryDeactivateArenaPerk_NotActive_ReturnsFalse()
        {
            var player = MakeSquireWithGold();

            bool result = player.TryDeactivateArenaPerk("not_active_perk");

            Assert.That(result, Is.False);
        }

        // --- InitializeArenaPerkStates ---

        [Test]
        public void InitializeArenaPerkStates_RebuildsStatesFromIds()
        {
            var player = MakeSquireWithGold();
            var perk = MakePerk();
            player.TryPurchaseArenaPerk(perk);
            player.TryActivateArenaPerk(perk.Id);

            // Simulate a save/load: clear runtime state and reinitialise
            player.ArenaPerkStates.Clear();
            player.InitializeArenaPerkStates();

            Assert.That(player.ArenaPerkStates.ContainsKey(perk.Id), Is.True);
            Assert.That(player.ArenaPerkStates[perk.Id].PerkId, Is.EqualTo(perk.Id));
        }

        // --- Save/Load round-trip ---

        [Test]
        public void SaveLoad_UnlockedAndActivePerks_Survive()
        {
            var player = MakeSquireWithGold();
            var perk = MakePerk();
            player.TryPurchaseArenaPerk(perk);
            player.TryActivateArenaPerk(perk.Id);

            // Serialise + deserialise
            var json = JsonConvert.SerializeObject(player);
            var reloaded = JsonConvert.DeserializeObject<Player>(json)!;
            reloaded.InitializeArenaPerkStates();

            Assert.That(reloaded.AcquiredPerks, Contains.Item(perk.Id));
            Assert.That(reloaded.ActiveArenaPerkIds, Contains.Item(perk.Id));
            Assert.That(reloaded.ArenaPerkStates.ContainsKey(perk.Id), Is.True);
        }
    }
}
