using NUnit.Framework;
using PocketSquire.Arena.Core.Town;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class TownLocationTests
    {
        [Test]
        public void DialogueAction_HasExpectedValues()
        {
            // Verify all expected enum values exist
            Assert.That(DialogueAction.None, Is.EqualTo((DialogueAction)0));
            Assert.That(DialogueAction.Leave, Is.EqualTo((DialogueAction)1));
            Assert.That(DialogueAction.Shop, Is.EqualTo((DialogueAction)2));
            Assert.That(DialogueAction.Train, Is.EqualTo((DialogueAction)3));
            Assert.That(DialogueAction.Talk, Is.EqualTo((DialogueAction)4));
            Assert.That(DialogueAction.Prepare, Is.EqualTo((DialogueAction)5));
        }

        [Test]
        public void DialogueOption_Constructor_SetsProperties()
        {
            var option = new DialogueOption("Test Button", DialogueAction.Shop);

            Assert.That(option.buttonText, Is.EqualTo("Test Button"));
            Assert.That(option.action, Is.EqualTo(DialogueAction.Shop));
        }

        [Test]
        public void DialogueOption_Leave_Factory_CreatesCorrectOption()
        {
            var option = DialogueOption.Leave();

            Assert.That(option.buttonText, Is.EqualTo("Leave"));
            Assert.That(option.action, Is.EqualTo(DialogueAction.Leave));
        }

        [Test]
        public void DialogueOption_Shop_Factory_CreatesCorrectOption()
        {
            var option = DialogueOption.Shop();

            Assert.That(option.buttonText, Is.EqualTo("Shop"));
            Assert.That(option.action, Is.EqualTo(DialogueAction.Shop));
        }

        [Test]
        public void DialogueOption_Train_Factory_CreatesCorrectOption()
        {
            var option = DialogueOption.Train();

            Assert.That(option.buttonText, Is.EqualTo("Train"));
            Assert.That(option.action, Is.EqualTo(DialogueAction.Train));
        }

        [Test]
        public void DialogueOption_Prepare_Factory_CreatesCorrectOption()
        {
            var option = DialogueOption.Prepare();

            Assert.That(option.buttonText, Is.EqualTo("Prepare"));
            Assert.That(option.action, Is.EqualTo(DialogueAction.Prepare));
        }

        [Test]
        public void DialogueOption_DefaultStruct_HasNoneAction()
        {
            var option = new DialogueOption();

            Assert.That(option.action, Is.EqualTo(DialogueAction.None));
            Assert.That(option.buttonText, Is.Null);
        }
    }
}
