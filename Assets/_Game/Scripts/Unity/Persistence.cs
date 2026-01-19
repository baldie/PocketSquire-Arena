using UnityEngine;
using System.IO;
using System;

public static class SaveSystem
{
    private static string GetSaveFilePath(PocketSquire.Arena.Core.SaveSlots slot)
    {
        var saveFileName = String.Format("savefile_{0}.json", slot);
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }
    public static void SaveGame(PocketSquire.Arena.Core.SaveSlots slot, SaveData data)
    {
        // 1. Convert the data object to a JSON string
        string json = JsonUtility.ToJson(data, true); // 'true' makes it pretty-print

        // 2. Define the path
        var path = GetSaveFilePath(slot);

        // 3. Write to file
        File.WriteAllText(path, json);
        
        Debug.Log("Game Saved to: " + path);
    }

    public static SaveData LoadGame(PocketSquire.Arena.Core.SaveSlots slot)
    {
        var path = GetSaveFilePath(slot);

        if (File.Exists(path))
        {
            // 1. Read the text from the file
            string json = File.ReadAllText(path);

            // 2. Convert JSON back to the SaveData object
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return data;
        }
        else
        {
            Debug.LogWarning("Save file not found in " + path);
            return null;
        }
    }
}