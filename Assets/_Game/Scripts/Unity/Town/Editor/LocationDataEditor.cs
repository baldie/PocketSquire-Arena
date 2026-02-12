using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PocketSquire.Arena.Unity.Town;

namespace PocketSquire.Arena.Unity.Town.Editor
{
    /// <summary>
    /// Custom editor for LocationData to provide a dropdown selection for shop items
    /// based on the items.json data file.
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
            // Use Application.dataPath to get to Assets folder
            string path = Path.Combine(Application.dataPath, "_Game/Data/items.json");
            
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    availableItems = JsonConvert.DeserializeObject<List<ItemData>>(json);
                    
                    if (availableItems == null)
                    {
                        availableItems = new List<ItemData>();
                    }
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

            // Prepare display names for the popup
            itemNames = new string[availableItems.Count];
            for (int i = 0; i < availableItems.Count; i++)
            {
                itemNames[i] = $"{availableItems[i].name} (ID: {availableItems[i].id})";
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw all default visible fields (Name, Visuals, Audio, Dialogue)
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shop Setup", EditorStyles.boldLabel);

            SerializedProperty shopItemIdsProp = serializedObject.FindProperty("shopItemIds");

            if (availableItems.Count == 0)
            {
                EditorGUILayout.HelpBox("No items found in items.json. Please ensure the file exists and is populated.", MessageType.Warning);
                if (GUILayout.Button("Try Reloading items.json"))
                {
                    LoadItems();
                }
                return;
            }

            // Draw each shop item row
            for (int i = 0; i < shopItemIdsProp.arraySize; i++)
            {
                SerializedProperty elementProp = shopItemIdsProp.GetArrayElementAtIndex(i);
                int currentId = elementProp.intValue;

                // Find index in available items
                int selectedIndex = availableItems.FindIndex(x => x.id == currentId);
                
                EditorGUILayout.BeginHorizontal();
                
                if (selectedIndex < 0)
                {
                    // Item ID not found in JSON (maybe it was removed)
                    EditorGUILayout.LabelField($"ID: {currentId} (NOT FOUND)", GUILayout.MinWidth(100));
                    selectedIndex = 0; // Default to first for fixing
                }

                // Display the dropdown
                int newIndex = EditorGUILayout.Popup($"Item {i + 1}", selectedIndex, itemNames);
                
                // Update ID if selection changed
                if (newIndex != selectedIndex || (selectedIndex == 0 && currentId != availableItems[0].id))
                {
                    elementProp.intValue = availableItems[newIndex].id;
                    currentId = availableItems[newIndex].id;
                    selectedIndex = newIndex;
                }

                // Sprite Preview
                if (selectedIndex >= 0)
                {
                    string spriteName = availableItems[selectedIndex].sprite;
                    if (!string.IsNullOrEmpty(spriteName))
                    {
                        // Try loading from the standard items folder
                        string spritePath = $"Assets/_Game/Art/Items/{spriteName}.png";
                        Sprite itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                        if (itemSprite != null)
                        {
                            // Display sprite preview
                            Texture2D texture = AssetPreview.GetAssetPreview(itemSprite);
                            if (texture != null)
                            {
                                GUILayout.Label(texture, GUILayout.Width(32), GUILayout.Height(32));
                            }
                            else
                            {
                                // Fallback if preview isn't ready
                                GUILayout.Label("", GUILayout.Width(32), GUILayout.Height(32));
                            }
                        }
                        else
                        {
                            // Placeholder for missing sprite
                            EditorGUILayout.HelpBox("?", MessageType.None, true);
                        }
                    }
                }

                // Remove button
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
                int newIndex = shopItemIdsProp.arraySize;
                shopItemIdsProp.InsertArrayElementAtIndex(newIndex);
                // Default to the first available item if we have any
                if (availableItems.Count > 0)
                {
                    shopItemIdsProp.GetArrayElementAtIndex(newIndex).intValue = availableItems[0].id;
                }
            }

            if (GUILayout.Button("Refresh Item List"))
            {
                LoadItems();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
