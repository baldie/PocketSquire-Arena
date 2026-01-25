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
            Assert.That(monster.GetActionSoundId(ActionType.UseItem), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetActionAnimationId_ReturnsStandardAnimationNames()
        {
            var monster = new Monster();
            
            Assert.That(monster.GetActionAnimationId(ActionType.Attack), Is.EqualTo("Attack"));
            Assert.That(monster.GetActionAnimationId(ActionType.Defend), Is.EqualTo("Defend"));
            Assert.That(monster.GetActionAnimationId(ActionType.Yield), Is.EqualTo("Yield"));
        }
        
        [Test]
        public void GetActionAnimationId_ReturnsIdleForByDefault()
        {
            var monster = new Monster();
            // UseItem falls through to default "Idle" in Monster implementation
            Assert.That(monster.GetActionAnimationId(ActionType.UseItem), Is.EqualTo("Idle"));
        }

        [Test]
        public void GetHitSoundId_ReturnsConfiguredHitSound()
        {
            var monster = new Monster();
            monster.HitSoundId = "squish";

            Assert.That(monster.GetHitSoundId(), Is.EqualTo("squish"));
        }

        [Test]
        public void GetHitAnimationId_ReturnsHit()
        {
            var monster = new Monster();
            Assert.That(monster.GetHitAnimationId(), Is.EqualTo("Hit"));
        }
    }
}
