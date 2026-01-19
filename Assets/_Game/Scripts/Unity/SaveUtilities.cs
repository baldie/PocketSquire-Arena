using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SaveUtilities
{
    [MenuItem("Tools/Clear Save File")]
    public static void ClearAllSaveData()
    {
        var slots = new[] {
            PocketSquire.Arena.Core.SaveSlots.Slot1,
            PocketSquire.Arena.Core.SaveSlots.Slot2,
            PocketSquire.Arena.Core.SaveSlots.Slot3
        };

        foreach (var slot in slots)
        {
            string path = SaveSystem.GetSaveFilePath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Save file deleted from: {path}");
            }
        }
    }
}