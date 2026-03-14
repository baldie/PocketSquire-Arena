using NUnit.Framework;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class LoseActionTests
    {
        [Test]
        public void LoseAction_Properties_CorrectlySet()
        {
            var actor = new Player("Loser", 0, 10, new Attributes(), Player.Genders.m);
            var target = new Monster("Winner", 10, 10, new Attributes());
            var action = new LoseAction(actor, target);

            Assert.That(action.Type, Is.EqualTo(ActionType.Lose));
            Assert.That(action.Actor, Is.EqualTo(actor));
            Assert.That(action.Target, Is.EqualTo(target));
        }

        [Test]
        public void LoseAction_ApplyEffect_ExecutesWithoutError()
        {
            var actor = new Player("Loser", 0, 10, new Attributes(), Player.Genders.m);
            var target = new Monster("Winner", 10, 10, new Attributes());
            var action = new LoseAction(actor, target);

            Assert.DoesNotThrow(() => action.ApplyEffect());
        }

        [Test]
        public void LoseAction_ApplyEffect_TriggersBattleLostPerks()
        {
            var actor = new Player("Loser", 0, 10, new Attributes(), Player.Genders.m);
            var target = new Monster("Winner", 10, 10, new Attributes());
            var perk = new Perk
            {
                Id = "battle_lost_test",
                DisplayName = "Battle Lost Test",
                PerkType = PerkType.Triggered,
                TriggerEvent = PerkTriggerEvent.BattleLost,
                Effect = PerkEffectType.IncreaseMaxHP,
                Value = 5
            };

            actor.AcquiredPerks.Add(perk);
            actor.ActivePerks.Add(perk);
            actor.PerkStates[perk.Id] = new PerkState { PerkId = perk.Id };

            var action = new LoseAction(actor, target);
            action.ApplyEffect();

            Assert.That(actor.MaxHealth, Is.EqualTo(15));
        }
    }
}
