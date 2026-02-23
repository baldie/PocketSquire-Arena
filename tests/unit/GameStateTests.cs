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
        GameState.Player = null;

        // Ensure GameWorld has the expected player for CreateNewGame
        GameWorld.ClassTemplates.Clear();
        var template = new Player("m_spellcaster", 20, 20, new Attributes(), Player.Genders.m);
        template.ChangeClass(PlayerClass.ClassName.SpellCaster);
        GameWorld.ClassTemplates.Add(template);
    }

    [Test]
    public void CreateNewGame_SetsCorrectSlotAndDates()
    {
        // Arrange
        var slot = SaveSlots.Slot1;

        // Act
        GameState.CreateNewGame(slot);

        // Assert
        Assert.That(GameState.SelectedSaveSlot, Is.EqualTo(slot));
        Assert.That(GameState.CharacterCreationDate, Is.Not.Null);
        Assert.That(GameState.LastSaveDate, Is.Not.Null);
        Assert.That(GameState.PlayTime, Is.EqualTo(TimeSpan.Zero));
        
        // Check that dates are approximately now (within 5 seconds)
        Assert.That((DateTime.Now - GameState.CharacterCreationDate!.Value).TotalSeconds, Is.LessThan(5));
        Assert.That((DateTime.Now - GameState.LastSaveDate!.Value).TotalSeconds, Is.LessThan(5));
        
        Assert.That(GameState.Player, Is.Not.Null);
        Assert.That(GameState.Player!.Name, Is.EqualTo("m_spellcaster"));
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
        Assert.That(saveData.CharacterCreationDate, Is.EqualTo(creationDate.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        Assert.That(saveData.LastSaveDateString, Is.EqualTo(lastSaveDate.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        Assert.That(saveData.PlayTimeTicks, Is.EqualTo(playTime.Ticks));
        Assert.That(saveData.Player, Is.Not.Null);
        Assert.That(saveData.Player!.Name, Is.EqualTo("m_spellcaster"));
    }

    [Test]
    public void LoadFromSaveData_SetsCorrectValues()
    {
        // Arrange
        var creationDate = new DateTime(2023, 1, 1);
        var lastSaveDate = new DateTime(2023, 1, 5);
        var playTime = TimeSpan.FromHours(10);

        var data = new SaveData
        {
            SelectedSaveSlot = SaveSlots.Slot3,
            CharacterCreationDate = creationDate.ToString(System.Globalization.CultureInfo.InvariantCulture),
            LastSaveDateString = lastSaveDate.ToString(System.Globalization.CultureInfo.InvariantCulture),
            PlayTimeTicks = playTime.Ticks,
            Player = new Player("Test Player", 20, 20, new Attributes(), Player.Genders.m)
        };

        // Act
        GameState.LoadFromSaveData(data);

        // Assert
        Assert.That(GameState.SelectedSaveSlot, Is.EqualTo(data.SelectedSaveSlot));
        Assert.That(GameState.CharacterCreationDate, Is.EqualTo(creationDate));
        Assert.That(GameState.LastSaveDate, Is.EqualTo(lastSaveDate));
        Assert.That(GameState.PlayTime, Is.EqualTo(playTime));
        Assert.That(GameState.Player, Is.Not.Null);
        Assert.That(GameState.Player!.Name, Is.EqualTo("Test Player"));
    }

    [Test]
    public void FindMostRecentSave_ReturnsLatestSave()
    {
        // Arrange
        var save1 = new SaveData { LastSaveDateString = new DateTime(2023, 1, 1).ToString(System.Globalization.CultureInfo.InvariantCulture) };
        var save2 = new SaveData { LastSaveDateString = new DateTime(2023, 1, 10).ToString(System.Globalization.CultureInfo.InvariantCulture) };
        var save3 = new SaveData { LastSaveDateString = new DateTime(2023, 1, 5).ToString(System.Globalization.CultureInfo.InvariantCulture) };
        
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
        var result = GameState.FindMostRecentSave(new SaveData?[] { });

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindMostRecentSave_WithNullElements_IgnoresThem()
    {
        // Arrange
        var save1 = new SaveData { LastSaveDateString = new DateTime(2023, 1, 1).ToString(System.Globalization.CultureInfo.InvariantCulture) };
        var saves = new SaveData?[] { null, save1, null };

        // Act
        var result = GameState.FindMostRecentSave(saves);

        // Assert
        Assert.That(result, Is.SameAs(save1));
    }

    [Test]
    public void CreateNewGame_GivesPlayerStartingHealthPotion()
    {
        // Arrange
        var slot = SaveSlots.Slot1;

        // Act
        GameState.CreateNewGame(slot);

        // Assert
        Assert.That(GameState.Player, Is.Not.Null);
        Assert.That(GameState.Player!.Inventory, Is.Not.Null);
        Assert.That(GameState.Player.Inventory.GetItemCount(1), Is.EqualTo(1), "Player should start with 1 Small Health Potion (id=1)");
    }

    [Test]
    public void SaveLoadRoundtrip_PreservesInventory()
    {
        // Arrange — start with 1 potion, add 1 more to fill slot 1 (stack 2), add item 2 to fill slot 2
        GameState.CreateNewGame(SaveSlots.Slot1);
        GameState.Player!.Inventory.AddItem(1, 1); // slot 1: qty 2 (full)
        GameState.Player.Inventory.AddItem(2, 2);  // slot 2: qty 2 (full)

        // Act - Save and load
        var saveData = GameState.GetSaveData();
        GameState.Player = null; // Clear state
        GameState.LoadFromSaveData(saveData);

        // Assert
        Assert.That(GameState.Player, Is.Not.Null);
        Assert.That(GameState.Player!.Inventory, Is.Not.Null);
        Assert.That(GameState.Player.Inventory.GetItemCount(1), Is.EqualTo(2), "Should have 1 starting + 1 added = 2 health potions");
        Assert.That(GameState.Player.Inventory.GetItemCount(2), Is.EqualTo(2), "Should have 2 of item 2");
    }
}
