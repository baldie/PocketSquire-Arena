using UnityEngine;
using TMPro;

namespace PocketSquire.Unity.UI
{
    /// <summary>
    /// MonoBehaviour for the SkillTree prefab.
    /// Closes the skill tree when the player presses Escape (Cancel action).
    /// Uses GameInput to avoid the deprecated legacy InputManager.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class SkillTreeController : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI hoverDescriptionText;

        private void Update()
        {
            // Only process input when this panel is actually open
            if (!gameObject.activeSelf)
                return;

            if (GameInput.Instance == null)
                return;

            if (GameInput.Instance.GetButtonDown(GameInput.Instance.CancelAction))
            {
                // Consume both Cancel and Pause so nothing else reacts to this press
                GameInput.Instance.ConsumeButton(GameInput.Instance.CancelAction);
                GameInput.Instance.ConsumeButton(GameInput.Instance.PauseAction);

                Close();
            }
        }

        /// <summary>
        /// Closes the skill tree by deactivating its GameObject.
        /// Can also be called directly from UI buttons (e.g. a close button).
        /// </summary>
        public void Close()
        {
            Debug.Log("Closing skill tree");
            gameObject.SetActive(false);
        }

        public void ShowHoverDescription(string description)
        {
            if (hoverDescriptionText != null)
            {
                hoverDescriptionText.text = description;
                // Optional: Ensure it's active if you want it toggled
                // hoverDescriptionText.gameObject.SetActive(true);
            }
        }

        public void HideHoverDescription()
        {
            if (hoverDescriptionText != null)
            {
                hoverDescriptionText.text = string.Empty;
                // hoverDescriptionText.gameObject.SetActive(false);
            }
        }
    }
}
