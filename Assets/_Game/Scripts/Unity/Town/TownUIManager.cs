using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PocketSquire.Arena.Core.Town;

namespace PocketSquire.Arena.Unity.Town
{
    /// <summary>
    /// Manages UI transitions between the town map and interior location panels.
    /// Populates the interior panel with data from LocationData ScriptableObjects.
    /// </summary>
    public class TownUIManager : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject townMapPanel;
        [SerializeField] private GameObject interiorPanel;

        [Header("Interior UI Elements")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI greetingText;
        [SerializeField] private Transform optionsContainer;

        [Header("Prefabs")]
        [SerializeField] private GameObject dialogueOptionButtonPrefab;

        private LocationData currentLocation;
        private readonly List<GameObject> spawnedButtons = new List<GameObject>();

        private void Start()
        {
            // Ensure we start on the town map
            if (interiorPanel != null)
            {
                interiorPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Transitions from the town map to the interior view for the specified location.
        /// </summary>
        public void ShowInterior(LocationData locationData)
        {
            if (locationData == null)
            {
                Debug.LogWarning("[TownUIManager] ShowInterior called with null LocationData");
                return;
            }

            currentLocation = locationData;

            // Populate UI elements
            if (backgroundImage != null)
            {
                backgroundImage.sprite = locationData.BackgroundSprite;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = locationData.NpcPortrait;
                portraitImage.gameObject.SetActive(locationData.NpcPortrait != null);
            }

            if (greetingText != null)
            {
                greetingText.text = locationData.InitialGreeting;
            }

            // Create dialogue option buttons
            PopulateOptions(locationData.DialogueOptions);

            // Switch panels
            if (townMapPanel != null)
            {
                townMapPanel.SetActive(false);
            }

            if (interiorPanel != null)
            {
                interiorPanel.SetActive(true);
            }

            Debug.Log($"[TownUIManager] Entered {locationData.LocationName}");
        }

        /// <summary>
        /// Returns to the town map from the interior view.
        /// </summary>
        public void ReturnToTown()
        {
            currentLocation = null;

            // Clear spawned buttons
            ClearOptions();

            // Switch panels
            if (interiorPanel != null)
            {
                interiorPanel.SetActive(false);
            }

            if (townMapPanel != null)
            {
                townMapPanel.SetActive(true);
            }

            Debug.Log("[TownUIManager] Returned to town map");
        }

        /// <summary>
        /// Handles a dialogue action triggered by a button click.
        /// </summary>
        public void HandleDialogueAction(DialogueAction action)
        {
            switch (action)
            {
                case DialogueAction.Leave:
                    ReturnToTown();
                    break;

                case DialogueAction.Shop:
                    Debug.Log($"[TownUIManager] Shop action triggered at {currentLocation?.LocationName}");
                    // TODO: Open shop interface
                    break;

                case DialogueAction.Train:
                    Debug.Log($"[TownUIManager] Train action triggered at {currentLocation?.LocationName}");
                    // TODO: Open training interface
                    break;

                case DialogueAction.Talk:
                    Debug.Log($"[TownUIManager] Talk action triggered at {currentLocation?.LocationName}");
                    // TODO: Advance dialogue
                    break;

                case DialogueAction.Prepare:
                    Debug.Log($"[TownUIManager] Prepare action triggered at {currentLocation?.LocationName}");
                    // TODO: Open preparation interface
                    break;

                default:
                    Debug.LogWarning($"[TownUIManager] Unhandled action: {action}");
                    break;
            }
        }

        private void PopulateOptions(IReadOnlyList<DialogueOption> options)
        {
            ClearOptions();

            if (optionsContainer == null || dialogueOptionButtonPrefab == null)
            {
                Debug.LogWarning("[TownUIManager] Missing optionsContainer or dialogueOptionButtonPrefab");
                return;
            }

            foreach (var option in options)
            {
                var buttonObj = Instantiate(dialogueOptionButtonPrefab, optionsContainer);
                spawnedButtons.Add(buttonObj);

                // Set button text
                var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = option.buttonText;
                }

                // Wire up click handler
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    var capturedAction = option.action;
                    button.onClick.AddListener(() => HandleDialogueAction(capturedAction));
                }
            }
        }

        private void ClearOptions()
        {
            foreach (var buttonObj in spawnedButtons)
            {
                if (buttonObj != null)
                {
                    Destroy(buttonObj);
                }
            }
            spawnedButtons.Clear();
        }
    }
}
