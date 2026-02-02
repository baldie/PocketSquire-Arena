using NUnit.Framework;
using PocketSquire.Arena.Core;
using System;

namespace PocketSquire.Arena.Tests;

[TestFixture]
public class PlaytimeTrackingTests
{
    [SetUp]
    public void Setup()
    {
        // Reset GameState before each test
        GameState.SelectedSaveSlot = SaveSlots.Unknown;
        GameState.PlayTime = null;
    }

    [Test]
    public void AccumulatePlaytime_WithNullPlayTime_InitializesToZero()
    {
        // Arrange
        GameState.PlayTime = null;
        var sessionDuration = TimeSpan.FromMinutes(10);

        // Act
        GameState.AccumulatePlaytime(sessionDuration);

        // Assert
        Assert.That(GameState.PlayTime, Is.Not.Null);
        Assert.That(GameState.PlayTime!.Value, Is.EqualTo(TimeSpan.FromMinutes(10)));
    }

    [Test]
    public void AccumulatePlaytime_WithExistingPlayTime_AddsSessionDuration()
    {
        // Arrange
        GameState.PlayTime = TimeSpan.FromMinutes(10);
        var sessionDuration = TimeSpan.FromMinutes(20);

        // Act
        GameState.AccumulatePlaytime(sessionDuration);

        // Assert
        Assert.That(GameState.PlayTime!.Value, Is.EqualTo(TimeSpan.FromMinutes(30)));
    }

    [Test]
    public void AccumulatePlaytime_MultipleSessions_AccumulatesCorrectly()
    {
        // Arrange
        GameState.PlayTime = TimeSpan.Zero;

        // Act - Simulate 3 sessions
        GameState.AccumulatePlaytime(TimeSpan.FromMinutes(5));
        GameState.AccumulatePlaytime(TimeSpan.FromMinutes(10));
        GameState.AccumulatePlaytime(TimeSpan.FromMinutes(15));

        // Assert
        Assert.That(GameState.PlayTime!.Value, Is.EqualTo(TimeSpan.FromMinutes(30)));
    }

    [Test]
    public void AccumulatePlaytime_WithZeroDuration_DoesNotChangePlayTime()
    {
        // Arrange
        GameState.PlayTime = TimeSpan.FromMinutes(10);

        // Act
        GameState.AccumulatePlaytime(TimeSpan.Zero);

        // Assert
        Assert.That(GameState.PlayTime!.Value, Is.EqualTo(TimeSpan.FromMinutes(10)));
    }

    [Test]
    public void GetSaveData_IncludesPlayTimeTicks()
    {
        // Arrange
        GameState.CreateNewGame(SaveSlots.Slot1);
        GameState.PlayTime = TimeSpan.FromMinutes(25);

        // Act
        var saveData = GameState.GetSaveData();

        // Assert
        Assert.That(saveData.PlayTimeTicks, Is.EqualTo(TimeSpan.FromMinutes(25).Ticks));
    }

    [Test]
    public void LoadFromSaveData_RestoresPlayTime()
    {
        // Arrange
        var saveData = new SaveData
        {
            SelectedSaveSlot = SaveSlots.Slot1,
            PlayTimeTicks = TimeSpan.FromMinutes(42).Ticks,
            CharacterCreationDate = DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture),
            LastSaveDateString = DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        // Act
        GameState.LoadFromSaveData(saveData);

        // Assert
        Assert.That(GameState.PlayTime, Is.Not.Null);
        Assert.That(GameState.PlayTime!.Value, Is.EqualTo(TimeSpan.FromMinutes(42)));
    }

    [Test]
    public void PlayTime_RoundTrip_PreservesValue()
    {
        // Arrange
        GameState.CreateNewGame(SaveSlots.Slot2);
        var originalPlayTime = TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(37)).Add(TimeSpan.FromSeconds(15));
        GameState.PlayTime = originalPlayTime;

        // Act - Save and load
        var saveData = GameState.GetSaveData();
        GameState.PlayTime = null; // Reset
        GameState.LoadFromSaveData(saveData);

        // Assert
        Assert.That(GameState.PlayTime!.Value, Is.EqualTo(originalPlayTime));
    }

    [Test]
    public void CreateNewGame_InitializesPlayTimeToZero()
    {
        // Act
        GameState.CreateNewGame(SaveSlots.Slot3);

        // Assert
        Assert.That(GameState.PlayTime, Is.Not.Null);
        Assert.That(GameState.PlayTime!.Value, Is.EqualTo(TimeSpan.Zero));
    }
}
