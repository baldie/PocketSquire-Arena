using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PocketSquire.Arena.Unity.Town;
using PocketSquire.Arena.Unity.LevelUp;

namespace PocketSquire.Arena.Unity.Town.Editor
{
    /// <summary>
    /// Custom editor for LocationData.
    /// Draws items with dropdown + sprite preview, and perk nodes with icon preview.
    /// </summary>
    [CustomEditor(typeof(LocationData))]
    public class LocationDataEditor : UnityEditor.Editor
    {
        private class ItemData
        {
            public int id;
            public string name;
            public string sprite;
        }

        private List<ItemData> availableItems = new List<ItemData>();
        private string[] itemNames;

        private void OnEnable()
        {
            LoadItems();
        }

        private void LoadItems()
        {
            string path = Path.Combine(Application.dataPath, "_Game/Data/items.json");

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    availableItems = JsonConvert.DeserializeObject<List<ItemData>>(json);
                    if (availableItems == null) availableItems = new List<ItemData>();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LocationDataEditor] Failed to load items.json: {e.Message}");
                    availableItems = new List<ItemData>();
                }
            }
            else
            {
                Debug.LogWarning($"[LocationDataEditor] items.json not found at {path}");
                availableItems = new List<ItemData>();
            }

            itemNames = new string[availableItems.Count];
            for (int i = 0; i < availableItems.Count; i++)
                itemNames[i] = $"{availableItems[i].name} (ID: {availableItems[i].id})";
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Iterate all serialized properties.
            SerializedProperty prop = serializedObject.GetIterator();
            prop.NextVisible(true); // skip script field

            while (prop.NextVisible(false))
            {
                EditorGUILayout.PropertyField(prop, true);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shop Setup", EditorStyles.boldLabel);

            SerializedProperty shopItemIdsProp = serializedObject.FindProperty("shopItemIds");

            if (availableItems.Count == 0)
            {
                EditorGUILayout.HelpBox("No items found in items.json.", MessageType.Warning);
                if (GUILayout.Button("Try Reloading items.json")) LoadItems();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            for (int i = 0; i < shopItemIdsProp.arraySize; i++)
            {
                SerializedProperty elementProp = shopItemIdsProp.GetArrayElementAtIndex(i);
                int currentId = elementProp.intValue;
                int selectedIndex = availableItems.FindIndex(x => x.id == currentId);

                EditorGUILayout.BeginHorizontal();

                if (selectedIndex < 0)
                {
                    EditorGUILayout.LabelField($"ID: {currentId} (NOT FOUND)", GUILayout.MinWidth(100));
                    selectedIndex = 0;
                }

                int newIndex = EditorGUILayout.Popup($"Item {i + 1}", selectedIndex, itemNames);

                if (newIndex != selectedIndex || (selectedIndex == 0 && currentId != availableItems[0].id))
                {
                    elementProp.intValue = availableItems[newIndex].id;
                    currentId = availableItems[newIndex].id;
                    selectedIndex = newIndex;
                }

                // Sprite preview
                if (selectedIndex >= 0)
                {
                    string spriteName = availableItems[selectedIndex].sprite;
                    if (!string.IsNullOrEmpty(spriteName))
                    {
                        string spritePath = $"Assets/_Game/Art/Items/{spriteName}.png";
                        Sprite itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        if (itemSprite != null)
                        {
                            Texture2D texture = AssetPreview.GetAssetPreview(itemSprite);
                            if (texture != null)
                                GUILayout.Label(texture, GUILayout.Width(32), GUILayout.Height(32));
                            else
                                GUILayout.Label("", GUILayout.Width(32), GUILayout.Height(32));
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("?", MessageType.None, true);
                        }
                    }
                }

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    shopItemIdsProp.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Shop Item"))
            {
                int newIdx = shopItemIdsProp.arraySize;
                shopItemIdsProp.InsertArrayElementAtIndex(newIdx);
                if (availableItems.Count > 0)
                    shopItemIdsProp.GetArrayElementAtIndex(newIdx).intValue = availableItems[0].id;
            }

            if (GUILayout.Button("Refresh Item List")) LoadItems();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
