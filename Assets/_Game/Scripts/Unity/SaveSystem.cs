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

    public static void SaveGame(PocketSquire.Arena.Core.SaveSlots slot, SaveData data)
    {
        // Validate slot is a valid save slot (Slot1, Slot2, or Slot3)
        if (slot != PocketSquire.Arena.Core.SaveSlots.Slot1 && 
            slot != PocketSquire.Arena.Core.SaveSlots.Slot2 && 
            slot != PocketSquire.Arena.Core.SaveSlots.Slot3)
        {
            Debug.LogError($"[SaveSystem] Cannot save to invalid slot: {slot}. Only Slot1, Slot2, and Slot3 are valid.");
            return;
        }

        // Accumulate current session playtime before saving
        var tracker = UnityEngine.Object.FindFirstObjectByType<PlaytimeTracker>();
        tracker?.SaveCurrentPlaytime();
        
        // 0. Set the last save date
        data.LastSaveDateString = DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture);

        // 1. Convert the data object to a JSON string using Newtonsoft for better serialization of properties
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        // 2. Define the path
        var path = GetSaveFilePath(slot);

        // 3. Write to file
        File.WriteAllText(path, json);
        
        Debug.Log("Game Saved to: " + path);
    }


    public static SaveData LoadGame(PocketSquire.Arena.Core.SaveSlots slot)
    {
        var path = GetSaveFilePath(slot);

        if (!File.Exists(path))
            return null;
        
        // 1. Read the text from the file
        string json = File.ReadAllText(path);

        // 2. Convert JSON back to the SaveData object
        return JsonConvert.DeserializeObject<SaveData>(json);
    }
}