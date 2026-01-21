using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(GameAssetRegistry))]
public class AssetRegistryEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        GameAssetRegistry registry = (GameAssetRegistry)target;

        if (GUILayout.Button("Sync Folders (Auto-Fill IDs)")) {
            SyncAssets<Sprite>(registry.sprites, "Assets/_Game/Art/Monsters");
            SyncAssets<AudioClip>(registry.sounds, "Assets/_Game/Audio/Monsters");
            EditorUtility.SetDirty(registry);
        }
    }

    void SyncAssets<T>(dynamic list, string path) where T : Object {
        list.Clear();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { path });
        foreach (string guid in guids) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            
            // This assumes your JSON ID matches the filename
            string id = Path.GetFileNameWithoutExtension(assetPath);
            
            // Add to the list
            if (typeof(T) == typeof(Sprite)) list.Add(new GameAssetRegistry.SpriteEntry { id = id, asset = asset as Sprite });
            if (typeof(T) == typeof(AudioClip)) list.Add(new GameAssetRegistry.AudioEntry { id = id, asset = asset as AudioClip });
        }
    }
}