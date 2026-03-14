using NUnit.Framework;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Perks;
using PocketSquire.Arena.Core.PowerUps;
using System;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class WinActionTests
    {
        [SetUp]
        public void Setup()
        {
            GameState.CurrentRun = null;
        }

        [Test]
        public void WinAction_Properties_CorrectlySet()
        {
            var actor = new Player("Winner", 10, 10, new Attributes(), Player.Genders.m);
            var target = new Monster("Loser", 0, 10, new Attributes());
            var action = new WinAction(actor, target);

            Assert.That(action.Type, Is.EqualTo(ActionType.Win));
            Assert.That(action.Actor, Is.EqualTo(actor));
            Assert.That(action.Target, Is.EqualTo(target));
        }

        [Test]
        public void WinAction_ApplyEffect_AppliesPowerUpBonusesAndBattleHeal()
        {
            var actor = new Player("Winner", 50, 100, new Attributes(), Player.Genders.m);
            var target = new Monster("Loser", 0, 10, new Attributes())
            {
                Experience = 100,
                Gold = 50
            };

            var passiveGoldPerk = new Perk
            {
                Id = "gold_gain_test",
                DisplayName = "Gold Test",
                PerkType = PerkType.Passive,
                Effect = PerkEffectType.IncreaseGoldGain,
                Value = 100
            };
            actor.ActivePerks.Add(passiveGoldPerk);

            GameWorld.AllMonsters.Clear();
            GameState.CurrentRun = Run.StartNewRun();

            var xpComponent = new LootModifierComponent(
                LootModifierComponent.LootType.Experience,
                10f,
                Rarity.Common,
                PowerUpRank.I);
            var goldComponent = new LootModifierComponent(
                LootModifierComponent.LootType.Gold,
                10f,
                Rarity.Common,
                PowerUpRank.I);
            var healComponent = new UtilityComponent(
                UtilityComponent.UtilityType.PartialHeal,
                10f,
                Rarity.Common,
                PowerUpRank.I);

            GameState.CurrentRun.PowerUps.Add(new PowerUp(xpComponent));
            GameState.CurrentRun.PowerUps.Add(new PowerUp(goldComponent));
            GameState.CurrentRun.PowerUps.Add(new PowerUp(healComponent));

            var action = new WinAction(actor, target);

            float xpMultiplier = 1f + (xpComponent.GetBonusValue(GameState.CurrentRun.ArenaRank) / 100f);
            float goldMultiplier = 2f * (1f + (goldComponent.GetBonusValue(GameState.CurrentRun.ArenaRank) / 100f));
            int expectedHeal = (int)Math.Round(actor.MaxHealth * (healComponent.ComputeValue(GameState.CurrentRun.ArenaRank) / 100f));

            action.ApplyEffect();

            Assert.That(actor.Experience, Is.EqualTo((int)(target.Experience * xpMultiplier)));
            Assert.That(actor.Gold, Is.EqualTo((int)(target.Gold * goldMultiplier)));
            Assert.That(actor.Health, Is.EqualTo(Math.Min(actor.MaxHealth, 50 + expectedHeal)));
            Assert.That(GameState.CurrentRun.ArenaRank, Is.EqualTo(2));
        }
    }
}
