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

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        private LocationData currentLocation;
        private readonly List<GameObject> spawnedButtons = new List<GameObject>();
        private Vector2 originalPortraitPos;

        private void Start()
        {
            // Capture original position
            if (portraitImage != null)
            {
                originalPortraitPos = portraitImage.rectTransform.anchoredPosition;
            }

            // Find and reparent the menu cursor so it follows OptionsContainer visibility
            if (optionsContainer != null)
            {
                // Find inactive objects using transform search if necessary
                GameObject cursor = null;
                if (interiorPanel != null)
                {
                    // Search in interiorPanel path
                    Transform t = interiorPanel.transform.Find("DialogueBox/OptionsCursor");
                    if (t != null) cursor = t.gameObject;
                }
                
                if (cursor == null) cursor = GameObject.Find("OptionsCursor");

                if (cursor != null)
                {
                    cursor.transform.SetParent(optionsContainer, false);
                    cursor.transform.SetAsLastSibling();
                    
                    // Ensure it ignores layout
                    var layout = cursor.GetComponent<LayoutElement>();
                    if (layout == null) layout = cursor.AddComponent<LayoutElement>();
                    layout.ignoreLayout = true;
                    
                    // Start hidden
                    cursor.SetActive(false);
                }
            }

            // Try to find global UI Audio if not assigned
            if (audioSource == null)
            {
                var uiAudioObj = GameObject.Find("UIAudio");
                if (uiAudioObj != null)
                {
                    audioSource = uiAudioObj.GetComponent<AudioSource>();
                }
            }

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
            if (locationData == null)
            {
                Debug.LogWarning("[TownUIManager] ShowInteriorWithTransition called with null LocationData");
                return;
            }

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
                PlayGreeting(locationData.InitialGreeting);
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
                    ReturnToTownWithTransition();
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
        /// Transitions back to the town map with animation.
        /// </summary>
        public void ReturnToTownWithTransition()
        {
            StartCoroutine(TransitionToTownCoroutine());
        }

        private IEnumerator TransitionToTownCoroutine()
        {
            // Ensure flash overlay starts invisible and renders on top (using same overlay as entry)
            if (transitionFlashOverlay != null)
            {
                transitionFlashOverlay.alpha = 0f;
                transitionFlashOverlay.gameObject.SetActive(true);
                transitionFlashOverlay.transform.SetAsLastSibling();
            }

            // 1. Build-up (Anticipation)
            // Fade white overlay in (0 -> 1)
            Sequence sequence = DOTween.Sequence();
            
            if (transitionFlashOverlay != null)
            {
                // Fade in to full opacity
                sequence.Append(transitionFlashOverlay.DOFade(1f, 0.15f));
            }
            
            // Simultaneously punch the interior panel scale (shrink by 10%)
            if (interiorPanel != null)
            {
                sequence.Join(interiorPanel.transform.DOPunchScale(Vector3.one * -0.1f, 0.15f, 5, 1));
            }

            // 2. The Switch (at peak opacity)
            sequence.AppendCallback(() => 
            {
                ReturnToTown();

                // 3. The Snap-Back (Resolution)
                // Set town map initial scale to 110%
                if (townMapPanel != null)
                {
                    townMapPanel.transform.localScale = Vector3.one * 1.1f;
                    
                    // Animate scale 1.1 -> 1.0
                    townMapPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
            });

            // Fade out the overlay
            if (transitionFlashOverlay != null)
            {
                sequence.Append(transitionFlashOverlay.DOFade(0f, 0.3f));
                
                sequence.OnComplete(() =>
                {
                    transitionFlashOverlay.gameObject.SetActive(false);
                });
            }

            yield return sequence.WaitForCompletion();
        }

        /// <summary>
        /// Coroutine that handles the animated transition to an interior location.
        /// 1. Punch scale on the clicked button
        /// 2. Flash white overlay (0 → 0.8 → 0)
        /// 3. Swap backgrounds at peak flash brightness
        /// </summary>
        private IEnumerator TransitionToInteriorCoroutine(LocationData locationData, GameObject clickedButton)
        {
            currentLocation = locationData;

            // Ensure flash overlay starts invisible and renders on top
            if (transitionFlashOverlay != null)
            {
                transitionFlashOverlay.alpha = 0f;
                transitionFlashOverlay.gameObject.SetActive(true);
                
                // CRITICAL: Move to end of hierarchy so it renders on TOP of all other UI
                transitionFlashOverlay.transform.SetAsLastSibling();
            }
            else
            {
                Debug.LogWarning("[TownUIManager] transitionFlashOverlay is NULL!");
            }

            // 1. Build-up (Anticipation)
            Sequence flashSequence = DOTween.Sequence();
            float halfFlash = flashDuration * 0.5f;

            if (transitionFlashOverlay != null)
            {
                // Fade in to peak
                flashSequence.Append(transitionFlashOverlay.DOFade(0.8f, halfFlash));
            }

            // Play entry sound immediately
            if (audioSource != null && locationData.EntrySound != null)
            {
                audioSource.PlayOneShot(locationData.EntrySound);
            }
                
            // Simultaneously punch the clicked button icon
            if (clickedButton != null)
            {
                flashSequence.Join(clickedButton.transform.DOPunchScale(Vector3.one * 0.2f, punchDuration, 2, 0.5f));
            }

            // Simultaneously punch the town map panel (shrink by 10%) - same as exit transition
            if (townMapPanel != null)
            {
                flashSequence.Join(townMapPanel.transform.DOPunchScale(Vector3.one * -0.1f, halfFlash, 5, 1));
            }
            
            // 2. The Switch (at peak flash)
            flashSequence.AppendCallback(() =>
            {
                // Populate UI elements
                if (backgroundImage != null)
                {
                    backgroundImage.sprite = locationData.BackgroundSprite;
                }

                if (portraitImage != null)
                {
                    portraitImage.sprite = locationData.NpcPortrait;
                    bool hasPortrait = locationData.NpcPortrait != null;
                    portraitImage.gameObject.SetActive(hasPortrait);

                    if (hasPortrait)
                    {
                        // Reset to off-screen right and tween in
                        portraitImage.rectTransform.anchoredPosition = originalPortraitPos + new Vector2(500f, 0f);
                        portraitImage.rectTransform.DOAnchorPos(originalPortraitPos, 0.4f).SetEase(Ease.OutCubic);
                    }
                }

                // Hide options container initially
                if (optionsContainer != null)
                {
                    optionsContainer.gameObject.SetActive(false);
                }

                // Switch panels
                if (townMapPanel != null)
                {
                    townMapPanel.SetActive(false);
                }

                if (interiorPanel != null)
                {
                    interiorPanel.SetActive(true);
                    
                    // 3. Snap-Back (Resolution)
                    // Set interior panel initial scale to 110%
                    interiorPanel.transform.localScale = Vector3.one * 1.1f;
                    
                    // Animate scale 1.1 -> 1.0
                    interiorPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
            });
                
            // Fade out from peak
            if (transitionFlashOverlay != null)
            {
                flashSequence.Append(transitionFlashOverlay.DOFade(0f, halfFlash));
                
                // Clean up after flash completes
                flashSequence.OnComplete(() =>
                {
                    if (transitionFlashOverlay != null)
                    {
                        transitionFlashOverlay.gameObject.SetActive(false);
                    }
                });
            }
            
            yield return flashSequence.WaitForCompletion();

            // After transition, play greeting and wait for it
            if (greetingText != null)
            {
                // Play greeting and wait for the tween
                Tweener greetingTween = PlayGreeting(locationData.InitialGreeting);
                if (greetingTween != null)
                {
                    yield return greetingTween.WaitForCompletion();
                }
            }

            // Show options after greeting finishes
            // Create dialogue option buttons
            PopulateOptions(locationData.DialogueOptions);

            if (optionsContainer != null)
            {
                optionsContainer.gameObject.SetActive(true);
            }
            
            // Auto-select first option
            if (spawnedButtons.Count > 0)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(spawnedButtons[0]);
            }
        }

        /// <summary>
        /// Animates the greeting text like a typewriter.
        /// </summary>
        private Tweener PlayGreeting(string message)
        {
            if (greetingText == null) return null;
            
            // Clear the text, then "type" it over 0.75 seconds
            greetingText.text = "";
            return DOTween.To(() => greetingText.text, x => greetingText.text = x, message, 0.75f).SetEase(Ease.Linear);
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

                // Add hover selection support
                var eventTrigger = buttonObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null) eventTrigger = buttonObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                entry.callback.AddListener((data) => {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(buttonObj);
                });
                eventTrigger.triggers.Add(entry);
            }
        }
    }
}
