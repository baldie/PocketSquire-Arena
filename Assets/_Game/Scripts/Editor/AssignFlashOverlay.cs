using UnityEngine;
using UnityEditor;
using PocketSquire.Arena.Unity.Town;

/// <summary>
/// One-time editor utility to assign the TransitionFlashOverlay reference
/// </summary>
public class AssignFlashOverlay
{
    [MenuItem("Tools/Assign Flash Overlay to TownUIManager")]
    public static void AssignOverlay()
    {
        // Find the Canvas with TownUIManager
        var townUIManager = GameObject.Find("Canvas")?.GetComponent<TownUIManager>();
        if (townUIManager == null)
        {
            Debug.LogError("[AssignFlashOverlay] Could not find TownUIManager on Canvas");
            return;
        }

        // Find the TransitionFlashOverlay by searching all CanvasGroups (including inactive)
        CanvasGroup flashOverlayCanvasGroup = null;
        var allCanvasGroups = Object.FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var cg in allCanvasGroups)
        {
            if (cg.gameObject.name == "TransitionFlashOverlay")
            {
                flashOverlayCanvasGroup = cg;
                break;
            }
        }

        if (flashOverlayCanvasGroup == null)
        {
            Debug.LogError("[AssignFlashOverlay] Could not find TransitionFlashOverlay CanvasGroup. Found " + allCanvasGroups.Length + " CanvasGroups total");
            foreach (var cg in allCanvasGroups)
            {
                Debug.Log("  - " + cg.gameObject.name);
            }
            return;
        }

        // Use SerializedObject to assign the reference (proper way in editor)
        var serializedManager = new SerializedObject(townUIManager);
        var flashOverlayProperty = serializedManager.FindProperty("transitionFlashOverlay");
        
        if (flashOverlayProperty != null)
        {
            flashOverlayProperty.objectReferenceValue = flashOverlayCanvasGroup;
            serializedManager.ApplyModifiedProperties();
            
            Debug.Log("[AssignFlashOverlay] SUCCESS! Flash overlay assigned to TownUIManager");
            EditorUtility.SetDirty(townUIManager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }
        else
        {
            Debug.LogError("[AssignFlashOverlay] Could not find transitionFlashOverlay property");
        }
    }
}
