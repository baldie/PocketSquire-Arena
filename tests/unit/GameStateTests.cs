using NUnit.Framework;
using PocketSquire.Arena.Core;
using System;

namespace PocketSquire.Arena.Tests;

[TestFixture]
public class GameStateTests
{
    [SetUp]
    public void Setup()
    {
        // Reset GameState before each test
        GameState.SelectedSaveSlot = SaveSlots.Unknown;
        GameState.CharacterCreationDate = null;
        GameState.LastSaveDate = null;
        GameState.PlayTime = null;
    }

    [Test]
    public void CreateNewGame_SetsCorrectSlotAndDates()
    {
        // Arrange
        var slot = SaveSlots.Slot1;
        var now = DateTime.Now;

        // Act
        GameState.CreateNewGame(slot);

        // Assert
        Assert.That(GameState.SelectedSaveSlot, Is.EqualTo(slot));
        Assert.That(GameState.CharacterCreationDate, Is.Not.Null);
        Assert.That(GameState.LastSaveDate, Is.Not.Null);
        Assert.That(GameState.PlayTime, Is.EqualTo(TimeSpan.Zero));
        
        // Check that dates are approximately now (within 5 seconds)
        Assert.That((DateTime.Now - GameState.CharacterCreationDate.Value).TotalSeconds, Is.LessThan(5));
        Assert.That((DateTime.Now - GameState.LastSaveDate.Value).TotalSeconds, Is.LessThan(5));
    }

    [Test]
    public void GetSaveData_ReturnsCorrectData()
    {
        // Arrange
        var slot = SaveSlots.Slot2;
        GameState.CreateNewGame(slot);
        
        // Manually set some values to be sure
        var creationDate = new DateTime(2023, 1, 1);
        var lastSaveDate = new DateTime(2023, 1, 2);
        var playTime = TimeSpan.FromHours(5);
        
        GameState.CharacterCreationDate = creationDate;
        GameState.LastSaveDate = lastSaveDate;
        GameState.PlayTime = playTime;

        // Act
        var saveData = GameState.GetSaveData();

        // Assert
        Assert.That(saveData.SelectedSaveSlot, Is.EqualTo(slot));
        Assert.That(saveData.CharacterCreationDate, Is.EqualTo(creationDate));
        Assert.That(saveData.LastSaveDate, Is.EqualTo(lastSaveDate));
        Assert.That(saveData.PlayTime, Is.EqualTo(playTime));
    }
}
