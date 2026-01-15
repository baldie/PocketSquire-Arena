using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class ArenaLogicTests
    {
        [Test]
        public void GetHelloMessage_ReturnsHelloArena()
        {
            // Arrange
            var arenaLogic = new ArenaLogic();

            // Act
            var result = arenaLogic.GetHelloMessage();

            // Assert
            Assert.AreEqual("Hello Arena", result);
        }

        [Test]
        public void GetHelloMessage_IsNotNullOrEmpty()
        {
            // Arrange
            var arenaLogic = new ArenaLogic();

            // Act
            var result = arenaLogic.GetHelloMessage();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }
    }
}
