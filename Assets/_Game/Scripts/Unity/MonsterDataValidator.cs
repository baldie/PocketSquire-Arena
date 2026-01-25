using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using PocketSquire.Arena.Core;
using Newtonsoft.Json;

public class MonsterDataValidator : EditorWindow
{
    [MenuItem("Tools/Validate Monster Data")]
    public static void Validate()
    {
        // 1. Load the Registry
        GameAssetRegistry registry = AssetDatabase.LoadAssetAtPath<GameAssetRegistry>("Assets/Data/MonsterAssetRegistry.asset");

        if (registry == null)
        {
            Debug.LogError("Validator: Could not find the GameAssetRegistry!");
            return;
        }

        // 2. Load the JSON
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "entities.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"Validator: JSON file not found at {jsonPath}");
            return;
        }

        string jsonContent = File.ReadAllText(jsonPath);
        
        // Use Newtonsoft.Json for consistency with GameWorld
        List<Monster> monsters = JsonConvert.DeserializeObject<List<Monster>>(jsonContent) ?? new List<Monster>();

        int errorCount = 0;

        // 3. The Cross-Reference Check
        foreach (var monster in monsters)
        {
            // Check Sprite
            if (registry.GetSprite(monster.SpriteId) == null)
            {
                Debug.LogError($"[Data Error] Monster '{monster.Name}' has invalid SpriteId: '{monster.SpriteId}'");
                errorCount++;
            }

            // Check Sounds
            CheckSound(monster.Name, "Attack", monster.AttackSoundId, registry, ref errorCount);
            CheckSound(monster.Name, "Defend", monster.DefendSoundId, registry, ref errorCount);
            CheckSound(monster.Name, "Hit", monster.HitSoundId, registry, ref errorCount);
        }

        if (errorCount == 0)
            Debug.Log("<color=green><b>Validation Success:</b> All monsters have valid assets!</color>");
        else
            Debug.Log($"<color=red><b>Validation Failed:</b> Found {errorCount} missing assets.</color>");
    }

    private static void CheckSound(string monsterName, string type, string soundId, GameAssetRegistry registry, ref int errorCount)
    {
        if (string.IsNullOrEmpty(soundId)) return; // Skip if sound isn't required

        if (registry.GetSound(soundId) == null)
        {
            Debug.LogError($"[Data Error] Monster '{monsterName}' has invalid {type}SoundId: '{soundId}'");
            errorCount++;
        }
    }
}