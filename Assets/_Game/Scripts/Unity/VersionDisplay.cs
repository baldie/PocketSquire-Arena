using UnityEngine;
using TMPro;

namespace PocketSquire.Unity
{
    /// <summary>
    /// Displays the current application version on a TextMeshProUGUI component.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VersionDisplay : MonoBehaviour
    {
        void Start()
        {
            // Get the text component
            var textMesh = GetComponent<TextMeshProUGUI>();
            
            if (textMesh != null)
            {
                // simple format: "v0.1.5"
                textMesh.text = $"v{Application.version}";
                
                // PRO TIP: Add a dev marker if this is a development build
                if (Debug.isDebugBuild)
                {
                    textMesh.text += " (Dev)";
                }
            }
            else
            {
                Debug.LogWarning($"[VersionDisplay] No TextMeshProUGUI component found on {gameObject.name}");
            }
        }
    }
}
