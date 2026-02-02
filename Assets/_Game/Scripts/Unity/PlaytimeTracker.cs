using UnityEngine;
using PocketSquire.Arena.Core;
using System;

/// <summary>
/// Singleton MonoBehaviour that tracks playtime for the active save slot.
/// Only tracks time when GameState.SelectedSaveSlot is not Unknown.
/// Persists across scene transitions using DontDestroyOnLoad.
/// </summary>
public class PlaytimeTracker : MonoBehaviour
{
    private DateTime? _sessionStartTime;
    private static PlaytimeTracker _instance;

    void Awake()
    {
        // Singleton pattern - ensure only one instance exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Start tracking playtime for the current session.
    /// Called when a save slot is selected (loaded or created).
    /// </summary>
    public void StartTracking()
    {
        if (GameState.SelectedSaveSlot == SaveSlots.Unknown)
        {
            Debug.LogWarning("[PlaytimeTracker] Cannot start tracking - no save slot selected");
            return;
        }

        _sessionStartTime = DateTime.UtcNow;
        Debug.Log($"[PlaytimeTracker] Started tracking for slot {GameState.SelectedSaveSlot}");
    }

    /// <summary>
    /// Stop tracking playtime (called when returning to main menu).
    /// </summary>
    public void StopTracking()
    {
        _sessionStartTime = null;
        Debug.Log("[PlaytimeTracker] Stopped tracking");
    }

    /// <summary>
    /// Get the current session duration since StartTracking() was called.
    /// Returns TimeSpan.Zero if not currently tracking.
    /// </summary>
    public TimeSpan GetCurrentSessionDuration()
    {
        if (_sessionStartTime == null)
            return TimeSpan.Zero;

        return DateTime.UtcNow - _sessionStartTime.Value;
    }

    /// <summary>
    /// Accumulate current session time into GameState.PlayTime and reset session timer.
    /// Called automatically by SaveSystem.SaveGame() before serialization.
    /// </summary>
    public void SaveCurrentPlaytime()
    {
        // Only accumulate if we're actively tracking
        if (_sessionStartTime == null)
            return;

        // Only accumulate if a valid save slot is selected
        if (GameState.SelectedSaveSlot == SaveSlots.Unknown)
            return;

        var sessionDuration = GetCurrentSessionDuration();
        
        if (sessionDuration.TotalSeconds > 0)
        {
            GameState.AccumulatePlaytime(sessionDuration);
            Debug.Log($"[PlaytimeTracker] Accumulated {sessionDuration.TotalMinutes:F2} minutes. Total: {GameState.PlayTime?.TotalMinutes:F2} minutes");
        }

        // Reset session timer to prevent double-counting
        _sessionStartTime = DateTime.UtcNow;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Save playtime when app is backgrounded (only if tracking)
        if (pauseStatus && _sessionStartTime != null)
        {
            SaveCurrentPlaytime();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Save playtime when window loses focus (only if tracking)
        if (!hasFocus && _sessionStartTime != null)
        {
            SaveCurrentPlaytime();
        }
    }

    void OnApplicationQuit()
    {
        // Final save before app closes (only if tracking)
        if (_sessionStartTime != null)
        {
            SaveCurrentPlaytime();
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
