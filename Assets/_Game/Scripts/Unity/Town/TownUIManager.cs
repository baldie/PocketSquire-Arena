using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PocketSquire.Arena.Core.Town;
using DG.Tweening;
using System.Collections;

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

        [Header("Transition Effects")]
        [SerializeField] private CanvasGroup transitionFlashOverlay;
        [SerializeField] private float punchDuration = 0.15f;
        [SerializeField] private float flashDuration = 0.2f;

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
        /// Transitions from the town map to the interior view with animation.
        /// </summary>
        /// <param name="locationData">The location to enter</param>
        /// <param name="clickedButton">The button that was clicked (for punch animation)</param>
        public void ShowInteriorWithTransition(LocationData locationData, GameObject clickedButton)
        {
            Debug.Log($"[TownUIManager] ShowInteriorWithTransition called for {locationData?.LocationName ?? "NULL"}, button: {clickedButton?.name ?? "NULL"}");
            
            if (locationData == null)
            {
                Debug.LogWarning("[TownUIManager] ShowInteriorWithTransition called with null LocationData");
                return;
            }

            Debug.Log($"[TownUIManager] Starting transition coroutine. FlashOverlay null? {transitionFlashOverlay == null}");
            StartCoroutine(TransitionToInteriorCoroutine(locationData, clickedButton));
        }

        /// <summary>
        /// Transitions from the town map to the interior view for the specified location.
        /// (Immediate transition without animation - kept for backward compatibility)
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

        /// <summary>
        /// Coroutine that handles the animated transition to an interior location.
        /// 1. Punch scale on the clicked button
        /// 2. Flash white overlay (0 → 0.8 → 0)
        /// 3. Swap backgrounds at peak flash brightness
        /// </summary>
        private IEnumerator TransitionToInteriorCoroutine(LocationData locationData, GameObject clickedButton)
        {
            Debug.Log($"[TownUIManager] TransitionToInteriorCoroutine started for {locationData.LocationName}");
            currentLocation = locationData;

            // Ensure flash overlay starts invisible and renders on top
            if (transitionFlashOverlay != null)
            {
                Debug.Log($"[TownUIManager] Setting flash overlay active. Current alpha: {transitionFlashOverlay.alpha}");
                transitionFlashOverlay.alpha = 0f;
                transitionFlashOverlay.gameObject.SetActive(true);
                
                // CRITICAL: Move to end of hierarchy so it renders on TOP of all other UI
                transitionFlashOverlay.transform.SetAsLastSibling();
                Debug.Log($"[TownUIManager] Flash overlay activated and moved to front. GameObject active: {transitionFlashOverlay.gameObject.activeSelf}");
            }
            else
            {
                Debug.LogWarning("[TownUIManager] transitionFlashOverlay is NULL!");
            }

            // 1. Punch the clicked button icon
            if (clickedButton != null)
            {
                Debug.Log($"[TownUIManager] Starting punch animation on button: {clickedButton.name}");
                clickedButton.transform.DOPunchScale(Vector3.one * 0.2f, punchDuration, 2, 0.5f);
            }
            else
            {
                Debug.LogWarning("[TownUIManager] clickedButton is NULL - no punch animation");
            }

            // 2. Start the flash animation (fade in to 0.8 over half duration)
            Sequence flashSequence = DOTween.Sequence();
            Debug.Log($"[TownUIManager] Creating DOTween sequence. Flash duration: {flashDuration}s");
            
            if (transitionFlashOverlay != null)
            {
                float halfFlash = flashDuration * 0.5f;
                Debug.Log($"[TownUIManager] Half flash duration: {halfFlash}s");
                
                // Fade in to peak
                Debug.Log("[TownUIManager] Appending fade IN to 0.8 alpha");
                flashSequence.Append(transitionFlashOverlay.DOFade(0.8f, halfFlash));
                
                // 3. At peak flash (brightest moment), swap the background
                flashSequence.AppendCallback(() =>
                {
                    Debug.Log($"[TownUIManager] PEAK FLASH - Swapping to {locationData.LocationName} interior");
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
                });
                
                // Fade out from peak
                Debug.Log("[TownUIManager] Appending fade OUT to 0 alpha");
                flashSequence.Append(transitionFlashOverlay.DOFade(0f, halfFlash));
                
                // Clean up after flash completes
                flashSequence.OnComplete(() =>
                {
                    Debug.Log("[TownUIManager] Flash sequence COMPLETE - deactivating overlay");
                    if (transitionFlashOverlay != null)
                    {
                        transitionFlashOverlay.gameObject.SetActive(false);
                    }
                });
            }
            else
            {
                Debug.LogWarning("[TownUIManager] Flash overlay is NULL - falling back to immediate transition");
                // Fallback if no flash overlay - just do immediate transition
                ShowInterior(locationData);
            }

            Debug.Log($"[TownUIManager] Coroutine ending for {locationData.LocationName}");
            
            yield return null;
        }
    }
}
