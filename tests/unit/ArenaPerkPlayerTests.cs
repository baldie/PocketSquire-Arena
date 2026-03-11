using NUnit.Framework;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Perks;
using Newtonsoft.Json;
using System.IO;
using System;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class PerkPlayerTests
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

        private Perk MakePerk(string id = "test_perk", int cost = 100)
        {
            return new Perk
            {
                Id = id,
                DisplayName = "Test Perk",
                Cost = cost,
                PerkType = PerkType.Passive,
                Vendor = VendorType.Shopkeeper
            };
        }

        // --- TryPurchasePerk ---

        [Test]
        public void TryPurchasePerk_Success_GoldDeductedAndIdAdded()
        {
            var player = MakeSquireWithGold(500);
            var perk = MakePerk(cost: 100);

            bool result = player.TryPurchasePerk(perk);

            Assert.That(result, Is.True);
            Assert.That(player.Gold, Is.EqualTo(400));
            Assert.That(player.AcquiredPerks.Any(p => p.Id == perk.Id), Is.True);
        }

        [Test]
        public void TryPurchasePerk_AlreadyOwned_ReturnsFalse()
        {
            var player = MakeSquireWithGold(500);
            var perk = MakePerk(cost: 100);
            player.TryPurchasePerk(perk);

            bool second = player.TryPurchasePerk(perk);

            Assert.That(second, Is.False);
            Assert.That(player.Gold, Is.EqualTo(400), "Gold should not be spent twice");
        }

        [Test]
        public void TryPurchasePerk_InsufficientGold_ReturnsFalse()
        {
            var player = MakeSquireWithGold(50);
            var perk = MakePerk(cost: 100);

            bool result = player.TryPurchasePerk(perk);

            Assert.That(result, Is.False);
            Assert.That(player.Gold, Is.EqualTo(50), "Gold unchanged on failure");
            Assert.That(player.AcquiredPerks.Any(p => p.Id == perk.Id), Is.False);
        }

        [Test]
        public void TryPurchasePerk_NullPerk_ThrowsArgumentNull()
        {
            var player = MakeSquireWithGold();
            Assert.Throws<ArgumentNullException>(() => player.TryPurchasePerk(null!));
        }

        // --- CanActivatePerk ---

        [Test]
        public void CanActivatePerk_AllowsSwap_WhenInventoryBelowFutureCapacity()
        {
            var player = MakeSquireWithGold();
            var perkToRemove = "satchel_tier_1";
            var perkToActivate = MakePerk("satchel_tier_2"); // Going to a bigger bag

            player.Inventory.AddItem(1, 1);
            player.Inventory.AddItem(2, 1);

            bool canSwap = player.CanActivatePerk(perkToRemove, perkToActivate);

            Assert.That(canSwap, Is.True);
        }

        [Test]
        public void CanActivatePerk_BlocksSwap_WhenInventoryAboveFutureCapacity()
        {
            var player = MakeSquireWithGold();
            
            // Assume the player currently has tier 1 and tier 1 gives 3 slots.
            player.ActivePerks.Add(MakePerk("satchel_tier_1"));
            player.Inventory.UpdateCapacity(player.ActivePerks);
            
            // Fill 3 slots
            player.Inventory.AddItem(1, 1);
            player.Inventory.AddItem(2, 1);
            player.Inventory.AddItem(3, 1);

            // Removing tier 1 will drop max capacity to base (2).
            
            // For dropping capacity on removal, PerkUI handles the explicit check.
            // But if we test CanActivatePerk, we can simulate swapping from tier 1 to a non-bag perk.
            var perkToActivate = MakePerk("combat_perk_1");
            bool canSwap = player.CanActivatePerk("satchel_tier_1", perkToActivate);

            Assert.That(canSwap, Is.False, "Should prevent removing bag when inventory has 3 slots filled.");
        }

        [Test]
        public void CanActivatePerk_AllowsSwap_SmallerToLarger_WhenCurrentAreFull()
        {
            var player = MakeSquireWithGold();
            
            // Assume the player currently has tier 1 and tier 1 gives 3 slots.
            player.ActivePerks.Add(MakePerk("satchel_tier_1"));
            player.Inventory.UpdateCapacity(player.ActivePerks);
            
            // Fill 3 slots
            player.Inventory.AddItem(1, 1);
            player.Inventory.AddItem(2, 1);
            player.Inventory.AddItem(3, 1);

            // Upgrading from tier 1 (3 slots) to tier 2 (4 slots).
            var perkToActivate = MakePerk("satchel_tier_2");
            bool canSwap = player.CanActivatePerk("satchel_tier_1", perkToActivate);

            Assert.That(canSwap, Is.True, "Should allow upgrading bag from 3 to 4 slots even when 3 are filled.");
        }

        [Test]
        public void CanActivatePerk_BlocksMultipleActiveSatchels_WhenAddingNewSatchel()
        {
            var player = MakeSquireWithGold();
            
            player.ActivePerks.Add(MakePerk("satchel_tier_1"));
            player.Inventory.UpdateCapacity(player.ActivePerks);

            var perkToActivate = MakePerk("satchel_tier_2");
            bool canSwap = player.CanActivatePerk(null, perkToActivate);

            Assert.That(canSwap, Is.False, "Should prevent having both tier 1 and tier 2 satchels active at the same time.");
        }

        [Test]
        public void CanActivatePerk_AllowsSwap_WhenReplacingSatchelWithAnotherSatchel()
        {
            var player = MakeSquireWithGold();
            
            player.ActivePerks.Add(MakePerk("satchel_tier_1"));
            player.Inventory.UpdateCapacity(player.ActivePerks);

            // Swapping out the existing satchel for another one (which is also an upgrade so capacity is fine).
            var perkToActivate = MakePerk("satchel_tier_2");
            bool canSwap = player.CanActivatePerk("satchel_tier_1", perkToActivate);

            Assert.That(canSwap, Is.True, "Should allow swapping a satchel for another satchel.");
        }

        // --- TryActivatePerk ---

        [Test]
        public void TryActivatePerk_Success_AddsToActiveAndCreatesState()
        {
            var player = MakeSquireWithGold();
            var perk = MakePerk();
            player.TryPurchasePerk(perk);

            bool result = player.TryActivatePerk(perk.Id);

            Assert.That(result, Is.True);
            Assert.That(player.ActivePerks.Any(p => p.Id == perk.Id), Is.True);
            Assert.That(player.PerkStates.ContainsKey(perk.Id), Is.True);
        }

        [Test]
        public void TryActivatePerk_NotOwned_ReturnsFalse()
        {
            var player = MakeSquireWithGold();

            bool result = player.TryActivatePerk("not_owned_perk");

            Assert.That(result, Is.False);
        }

        [Test]
        public void TryActivatePerk_Squire_CapIs2_ThirdActivationFails()
        {
            var player = MakeSquireWithGold(10000);
            Assert.That(player.MaxPerkSlots, Is.EqualTo(2), "Squire should have 2 slots");

            var p1 = MakePerk("perk_1"); var p2 = MakePerk("perk_2"); var p3 = MakePerk("perk_3");
            player.TryPurchasePerk(p1);
            player.TryPurchasePerk(p2);
            player.TryPurchasePerk(p3);

            player.TryActivatePerk(p1.Id);
            player.TryActivatePerk(p2.Id);
            bool third = player.TryActivatePerk(p3.Id);

            Assert.That(third, Is.False, "Should not activate beyond cap");
        }

        [Test]
        public void TryActivatePerk_AlreadyActive_ReturnsFalse()
        {
            var player = MakeSquireWithGold();
            var perk = MakePerk();
            player.TryPurchasePerk(perk);
            player.TryActivatePerk(perk.Id);

            bool second = player.TryActivatePerk(perk.Id);

            Assert.That(second, Is.False);
        }

        [Test]
        public void MaxPerkSlots_ChangesWithClass()
        {
            var player = MakeSquireWithGold();
            Assert.That(player.MaxPerkSlots, Is.EqualTo(2)); // Squire = tier 0 = 2 slots

            // Simulate class change — directly test GetMaxPerkSlots
            Assert.That(PlayerClass.GetMaxPerkSlots(PlayerClass.ClassName.Fighter), Is.EqualTo(4));
            Assert.That(PlayerClass.GetMaxPerkSlots(PlayerClass.ClassName.Mage), Is.EqualTo(6));
        }

        // --- TryDeactivatePerk ---

        [Test]
        public void TryDeactivatePerk_Success_RemovesFromActiveAndState()
        {
            var player = MakeSquireWithGold();
            var perk = MakePerk();
            player.TryPurchasePerk(perk);
            player.TryActivatePerk(perk.Id);

            bool result = player.TryDeactivatePerk(perk.Id);

            Assert.That(result, Is.True);
            Assert.That(player.ActivePerks.Any(p => p.Id == perk.Id), Is.False);
            Assert.That(player.PerkStates.ContainsKey(perk.Id), Is.False);
        }

        [Test]
        public void TryDeactivatePerk_NotActive_ReturnsFalse()
        {
            var player = MakeSquireWithGold();

            bool result = player.TryDeactivatePerk("not_active_perk");

            Assert.That(result, Is.False);
        }

        // --- InitializePerkStates ---

        [Test]
        public void InitializePerkStates_RebuildsStatesFromIds()
        {
            var player = MakeSquireWithGold();
            var perk = MakePerk();
            player.TryPurchasePerk(perk);
            player.TryActivatePerk(perk.Id);

            // Simulate a save/load: clear runtime state and reinitialise
            player.PerkStates.Clear();
            player.InitializePerkStates();

            Assert.That(player.PerkStates.ContainsKey(perk.Id), Is.True);
            Assert.That(player.PerkStates[perk.Id].PerkId, Is.EqualTo(perk.Id));
        }

        // --- Save/Load round-trip ---

        [Test]
        public void SaveLoad_UnlockedAndActivePerks_Survive()
        {
            var player = MakeSquireWithGold(1000);
            var perk = GameWorld.GetPerkById("satchel_tier_1");
            player.TryPurchasePerk(perk);
            player.TryActivatePerk(perk.Id);

            // Serialise + deserialise
            var json = JsonConvert.SerializeObject(player);
            var reloaded = JsonConvert.DeserializeObject<Player>(json)!;
            reloaded.InitializePerkStates();

            Assert.That(reloaded.AcquiredPerks.Any(p => p.Id == perk.Id), Is.True);
            Assert.That(reloaded.ActivePerks.Any(p => p.Id == perk.Id), Is.True);
            Assert.That(reloaded.PerkStates.ContainsKey(perk.Id), Is.True);
        }
    }
}
