using NUnit.Framework;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Tests;

[TestFixture]
public class ArenaLogicTests
{
    [Test]
    public void GetArenaName_ReturnsPocketSquireArena()
    {
        // Arrange
        var arenaLogic = new ArenaLogic();

        // Act
        var result = arenaLogic.GetArenaName();

        // Assert
        Assert.That(result, Is.EqualTo("Pocket Squire Arena"));
    }

    [Test]
    public void GetArenaName_IsNotNullOrEmpty()
    {
        // Arrange
        var arenaLogic = new ArenaLogic();

        // Act
        var result = arenaLogic.GetArenaName();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
    }
}
