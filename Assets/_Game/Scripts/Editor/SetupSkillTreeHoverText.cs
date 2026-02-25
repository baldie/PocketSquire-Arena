#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using PocketSquire.Unity.UI;
using PocketSquire.Arena.Core;
using System;

namespace PocketSquire.EditorScripts
{
    public class SetupClassTreeHoverText
    {
        [MenuItem("PocketSquire/Setup Skill Tree Hover Text")]
        public static void Setup()
        {
            string prefabPath = "Assets/_Game/Prefabs/ClassTree.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("Could not find Skill Tree prefab at " + prefabPath);
                return;
            }

            // Open prefab for editing
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            
            ClassTreeController controller = instance.GetComponent<ClassTreeController>();
            
            // 1. Create the TextMeshProUGUI object
            Transform container = instance.transform.Find("ClassTreeContainer");
            if (container == null)
            {
                Debug.LogError("Could not find ClassTreeContainer");
                UnityEngine.Object.DestroyImmediate(instance);
                return;
            }

            // We make HoverDescriptionText a child of the root (instance) instead of container
            // because ClassTreeContainer lacks a RectTransform, which breaks anchoring.
            Transform existingText = instance.transform.Find("HoverDescriptionText");
            if (existingText == null)
            {
                // Fallback: check if it's currently under container and move it
                existingText = container.Find("HoverDescriptionText");
                if (existingText != null) existingText.SetParent(instance.transform, false);
            }

            GameObject textObj;
            TextMeshProUGUI tmp;
            if (existingText == null)
            {
                textObj = new GameObject("HoverDescriptionText");
                textObj.transform.SetParent(instance.transform, false);
                tmp = textObj.AddComponent<TextMeshProUGUI>();
            }
            else
            {
                textObj = existingText.gameObject;
                tmp = textObj.GetComponent<TextMeshProUGUI>();
            }

            // Set RectTransform to stretch horizontally at the bottom
            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0); // Bottom left
            rt.anchorMax = new Vector2(1, 0); // Bottom right
            rt.anchoredPosition = new Vector2(0, 64); // 64 units up from bottom
            // Offset left/right by 0
            rt.offsetMin = new Vector2(0, rt.offsetMin.y);
            rt.offsetMax = new Vector2(0, rt.offsetMax.y);
            rt.sizeDelta = new Vector2(0, 50); // Height 50, stretching width
            rt.pivot = new Vector2(0.5f, 0.5f);

            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;
            tmp.text = "";

            controller.hoverDescriptionText = tmp;

            // 2. Set nodeClass on all nodes
            Transform nodesContainer = container.Find("NodesContainer");
            if (nodesContainer != null)
            {
                NodeController[] nodes = nodesContainer.GetComponentsInChildren<NodeController>(true);
                foreach(var node in nodes)
                {
                    string name = node.gameObject.name;
                    if (name.StartsWith("Node_"))
                    {
                        string classNameStr = name.Substring(5); // removes "Node_"
                        if (Enum.TryParse<PlayerClass.ClassName>(classNameStr, out var cName))
                        {
                            node.nodeClass = cName;
                            EditorUtility.SetDirty(node);
                        }
                    }
                }
            }

            EditorUtility.SetDirty(controller);
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            UnityEngine.Object.DestroyImmediate(instance);

            Debug.Log("Skill Tree setup complete!");
        }
    }
}
#endif
