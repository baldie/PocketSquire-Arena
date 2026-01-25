using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class LoseActionTests
    {
        [Test]
        public void LoseAction_Properties_CorrectlySet()
        {
            var actor = new Player("Loser", 0, 10, new Attributes(), Player.CharGender.m);
            var target = new Monster("Winner", 10, 10, new Attributes());
            var action = new LoseAction(actor, target);

            Assert.That(action.Type, Is.EqualTo(ActionType.Lose));
            Assert.That(action.Actor, Is.EqualTo(actor));
            Assert.That(action.Target, Is.EqualTo(target));
        }

        [Test]
        public void LoseAction_ApplyEffect_ExecutesWithoutError()
        {
            var actor = new Player("Loser", 0, 10, new Attributes(), Player.CharGender.m);
            var target = new Monster("Winner", 10, 10, new Attributes());
            var action = new LoseAction(actor, target);

            // Currently ApplyEffect is a placeholder, so we just ensure it doesn't throw.
            Assert.DoesNotThrow(() => action.ApplyEffect());
        }
    }
}
