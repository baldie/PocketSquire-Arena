using UnityEngine;
using UnityEditor;
using System.Linq;

public class CleanupFlashOverlays
{
    [MenuItem("Tools/Cleanup Flash Overlays")]
    public static void Cleanup()
    {
        var overlays = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => go.name == "TransitionFlashOverlay" && go.transform.root.name == "Canvas")
            .ToList();

        if (overlays.Count == 0)
        {
            Debug.Log("[CleanupFlashOverlays] No 'TransitionFlashOverlay' found in Canvas.");
            return;
        }

        foreach (var overlay in overlays)
        {
            // Only destroy if it looks like what we expect (e.g. child of Canvas or in scene)
            if (overlay.scene.IsValid()) 
            {
                Debug.Log($"[CleanupFlashOverlays] Destroying duplicate: {overlay.name} (InstanceID: {overlay.GetInstanceID()})");
                Object.DestroyImmediate(overlay);
            }
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }
}
