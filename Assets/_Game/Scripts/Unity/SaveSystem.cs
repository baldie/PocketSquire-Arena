using UnityEngine;
using System.IO;
using System;
using PocketSquire.Arena.Core;
using Newtonsoft.Json;

public static class SaveSystem
{
    public static string GetSaveFilePath(PocketSquire.Arena.Core.SaveSlots slot)
    {
        var saveFileName = String.Format("savefile_{0}.json", slot);
        return Path.Combine(Application.persistentDataPath, saveFileName).Replace('\\', '/');
    }

    /// <summary>
    /// Saves the game to the specified slot.
    /// Automatically accumulates current playtime and retrieves the latest GameState data.
    /// </summary>
    public static void SaveGame(PocketSquire.Arena.Core.SaveSlots slot)
    {
        // Validate slot is a valid save slot (Slot1, Slot2, or Slot3)
        if (slot != PocketSquire.Arena.Core.SaveSlots.Slot1 && 
            slot != PocketSquire.Arena.Core.SaveSlots.Slot2 && 
            slot != PocketSquire.Arena.Core.SaveSlots.Slot3)
        {
            Debug.LogError($"[SaveSystem] Cannot save to invalid slot: {slot}. Only Slot1, Slot2, and Slot3 are valid.");
            return;
        }

        // 1. Accumulate current session playtime BEFORE creating SaveData
        var tracker = UnityEngine.Object.FindFirstObjectByType<PlaytimeTracker>();
        tracker?.SaveCurrentPlaytime();

        // 2. Now get the SaveData with updated PlayTime
        var data = GameState.GetSaveData();
        
        // 3. Set the last save date
        data.LastSaveDateString = DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture);

        // 4. Convert the data object to a JSON string using Newtonsoft for better serialization of properties
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);

        // 5. Define the path
        var path = GetSaveFilePath(slot);

        // 6. Write to file
        File.WriteAllText(path, json);
        
        Debug.Log("Game Saved to: " + path);
    }

    public static SaveData LoadGame(PocketSquire.Arena.Core.SaveSlots slot)
    {
        var path = GetSaveFilePath(slot);

        if (!File.Exists(path))
            return null;
        
        // 1. Read the text from the file
        var json = File.ReadAllText(path);

        // 2. Convert JSON back to the SaveData object
        return JsonConvert.DeserializeObject<SaveData>(json);
    }
}