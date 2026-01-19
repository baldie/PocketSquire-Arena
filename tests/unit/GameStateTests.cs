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
        GameState.LastSaveDateString = null;
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
        Assert.That(GameState.LastSaveDateString, Is.Not.Null);
        Assert.That(GameState.PlayTime, Is.EqualTo(TimeSpan.Zero));
        
        // Check that dates are approximately now (within 5 seconds)
        Assert.That((DateTime.Now - GameState.CharacterCreationDate.Value).TotalSeconds, Is.LessThan(5));
        Assert.That((DateTime.Now - DateTime.Parse(GameState.LastSaveDateString)).TotalSeconds, Is.LessThan(5));
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
        GameState.LastSaveDateString = lastSaveDate.ToString();
        GameState.PlayTime = playTime;

        // Act
        var saveData = GameState.GetSaveData();

        // Assert
        Assert.That(saveData.SelectedSaveSlot, Is.EqualTo(slot));
        Assert.That(saveData.CharacterCreationDate, Is.EqualTo(creationDate));
        Assert.That(saveData.LastSaveDateString, Is.EqualTo(lastSaveDate.ToString()));
        Assert.That(saveData.PlayTime, Is.EqualTo(playTime));
    }

    [Test]
    public void LoadFromSaveData_SetsCorrectValues()
    {
        // Arrange
        var data = new SaveData
        {
            SelectedSaveSlot = SaveSlots.Slot3,
            CharacterCreationDate = new DateTime(2023, 1, 1),
            LastSaveDateString = new DateTime(2023, 1, 5).ToString(),
            PlayTime = TimeSpan.FromHours(10)
        };

        // Act
        GameState.LoadFromSaveData(data);

        // Assert
        Assert.That(GameState.SelectedSaveSlot, Is.EqualTo(data.SelectedSaveSlot));
        Assert.That(GameState.CharacterCreationDate, Is.EqualTo(data.CharacterCreationDate));
        Assert.That(GameState.LastSaveDateString, Is.EqualTo(data.LastSaveDateString));
        Assert.That(GameState.PlayTime, Is.EqualTo(data.PlayTime));
    }

    [Test]
    public void FindMostRecentSave_ReturnsLatestSave()
    {
        // Arrange
        var save1 = new SaveData { LastSaveDateString = new DateTime(2023, 1, 1).ToString() };
        var save2 = new SaveData { LastSaveDateString = new DateTime(2023, 1, 10).ToString() };
        var save3 = new SaveData { LastSaveDateString = new DateTime(2023, 1, 5).ToString() };
        
        var saves = new SaveData[] { save1, save2, save3 };

        // Act
        var result = GameState.FindMostRecentSave(saves);

        // Assert
        Assert.That(result, Is.SameAs(save2));
    }

    [Test]
    public void FindMostRecentSave_WithEmptyArray_ReturnsNull()
    {
        // Act
        var result = GameState.FindMostRecentSave(new SaveData[0]);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindMostRecentSave_WithNullElements_IgnoresThem()
    {
        // Arrange
        var save1 = new SaveData { LastSaveDateString = new DateTime(2023, 1, 1).ToString() };
        var saves = new SaveData[] { null, save1, null };

        // Act
        var result = GameState.FindMostRecentSave(saves);

        // Assert
        Assert.That(result, Is.SameAs(save1));
    }
}
