using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameAssetRegistry))]
public class AssetRegistryEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        GameAssetRegistry registry = (GameAssetRegistry)target;

        if (GUILayout.Button("Sync Folders (Auto-Fill IDs)")) {
            registry.PopulateAll();
        }
    }
}