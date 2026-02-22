using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AssetRegistryAutobinder
{
    // This constructor is called as soon as Unity loads or scripts recompile
    static AssetRegistryAutobinder()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // We only want to run this right as we leave Edit Mode to enter Play Mode
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            UpdateAllRegistries();
        }
    }

    private static void UpdateAllRegistries()
    {
        // This finds every instance of your GameAssetRegistry in the project
        string[] guids = AssetDatabase.FindAssets("t:GameAssetRegistry");
        
        if (guids.Length == 0) return;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameAssetRegistry registry = AssetDatabase.LoadAssetAtPath<GameAssetRegistry>(path);

            if (registry != null)
            {
                Debug.Log($"<color=cyan>Auto-Populating Assets for: {registry.name}</color>");
                registry.PopulateAll();
            }
        }
    }
}