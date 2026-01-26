using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class MonsterTests
    {
        [Test]
        public void GetActionSoundId_ReturnsConfiguredSoundIds()
        {
            var monster = new Monster();
            monster.AttackSoundId = "roar_loud";
            monster.DefendSoundId = "shield_clank";

            Assert.That(monster.GetActionSoundId(ActionType.Attack), Is.EqualTo("roar_loud"));
            Assert.That(monster.GetActionSoundId(ActionType.Defend), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetActionSoundId_ReturnsEmptyForYield()
        {
             var monster = new Monster();
             Assert.That(monster.GetActionSoundId(ActionType.Yield), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetActionSoundId_ReturnsEmptyForUnknownAction()
        {
            var monster = new Monster();
            // UseItem is not explicitly handled for sound in Monster, so it should be empty
            Assert.That(monster.GetActionSoundId(ActionType.Item), Is.EqualTo(string.Empty));
        }


        


        [Test]
        public void GetHitSoundId_ReturnsConfiguredHitSound()
        {
            var monster = new Monster();
            monster.HitSoundId = "squish";

            Assert.That(monster.GetHitSoundId(), Is.EqualTo("squish"));
        }
        [Test]
        public void Monster_SpriteIds_ReturnCorrectFormattedStrings()
        {
            var monster = new Monster("Orc Warrior", 10, 10, new Attributes());

            Assert.That(monster.SpriteId, Is.EqualTo("orc_warrior_battle"));
            Assert.That(monster.AttackSpriteId, Is.EqualTo("orc_warrior_attack"));
            Assert.That(monster.DefendSpriteId, Is.EqualTo("orc_warrior_defend"));
            Assert.That(monster.HitSpriteId, Is.EqualTo("orc_warrior_hit"));
        }
    }
}
