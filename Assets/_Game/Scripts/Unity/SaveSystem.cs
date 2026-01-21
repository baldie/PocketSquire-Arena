using UnityEngine;
using System.IO;
using System;

public static class SaveSystem
{
    public static string GetSaveFilePath(PocketSquire.Arena.Core.SaveSlots slot)
    {
        var saveFileName = String.Format("savefile_{0}.json", slot);
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }

    public static void SaveGame(PocketSquire.Arena.Core.SaveSlots slot, SaveData data)
    {
        // 0. Set the last save date
        data.LastSaveDate = DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture);

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

        if (!File.Exists(path))
            return null;
        
        // 1. Read the text from the file
        string json = File.ReadAllText(path);
        Debug.Log("Save file JSON loaded:\n" + json);

        // 2. Convert JSON back to the SaveData object
        return JsonUtility.FromJson<SaveData>(json);
    }
}