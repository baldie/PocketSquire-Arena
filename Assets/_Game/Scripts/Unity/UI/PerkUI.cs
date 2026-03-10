using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Perks;
using System.Collections.Generic;
using System.Linq;      

namespace PocketSquire.Arena.Unity.UI
{
    /// <summary>
    /// Represents a single perk slot in the PerksContainer.
    /// Clicking opens AcquiredPerkList so the player can swap the perk.
    /// Slots beyond MaxArenaPerkSlots are visually locked and non-interactive.
    /// </summary>
    public class PerkUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image perkIcon;
        [SerializeField] private Button button;
        [SerializeField] private AcquiredPerkListController perkListPanel;
        [SerializeField] private AudioSource audioSource;

        // Optional: shown while hovering — can be wired from PlayerMenuController.
        public TextMeshProUGUI perkDescriptionText;
        private ArenaPerk _loadedPerk;
        private string _assignedPerkId; // null/empty = empty slot

        public bool HasAssignedPerk => !string.IsNullOrEmpty(_assignedPerkId);

        private void Awake()
        {
            if (perkIcon == null)
            {
                var iconTransform = transform.Find("PerkIcon");
                if (iconTransform != null)
                    perkIcon = iconTransform.GetComponent<Image>();
            }

            if (button == null)
                button = GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(OnSlotClicked);
        }

        // -----------------------------------------------------------------------
        // Public surface called by PlayerMenuController
        // -----------------------------------------------------------------------

        /// <summary>Loads and displays a specific perk in this slot.</summary>
        public void LoadPerk(ArenaPerk perk)
        {
            _loadedPerk = perk;
            _assignedPerkId = perk?.Id;

            if (perkIcon == null) return;

            if (perk != null && !string.IsNullOrEmpty(perk.Icon))
            {
                var sprite = GameAssetRegistry.Instance.GetSprite(perk.Icon);
                if (sprite != null)
                {
                    perkIcon.sprite = sprite;
                    return;
                }
            }

            // Fallback: perk has no icon — show empty state.
            LoadEmpty();
        }

        /// <summary>
        /// Puts the slot into an "empty but available" state.
        /// Full-opacity white so the slot looks inviting to click.
        /// </summary>
        public void LoadEmpty()
        {
            _loadedPerk = null;
            _assignedPerkId = null;

            if (perkIcon == null) return;

            var emptySprite = GameAssetRegistry.Instance.GetSprite("empty");
            perkIcon.sprite = emptySprite;
        }

        /// <summary>
        /// Puts the slot into a "locked" state.
        /// </summary>
        public void LoadLocked()
        {
            _loadedPerk = null;
            _assignedPerkId = null;

            if (perkIcon == null) return;

            var lockedSprite = GameAssetRegistry.Instance.GetSprite("locked");
            if (lockedSprite != null)
            {
                perkIcon.sprite = lockedSprite;
            }
            else
            {
                perkIcon.sprite = GameAssetRegistry.Instance.GetSprite("empty");
            }
        }

        /// <summary>
        /// Controls whether this slot is interactive.
        /// Pass false for slots beyond the player's MaxArenaPerkSlots.
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (button != null)
                button.interactable = interactable;

            if (perkIcon != null)
            {
                perkIcon.material = null;
            }
        }

        /// <summary>
        /// Injects the shared AcquiredPerkList panel reference.
        /// Called by PlayerMenuController.RefreshPerks().
        /// </summary>
        public void SetPerkListPanel(AcquiredPerkListController panel)
        {
            perkListPanel = panel;
        }

        // -----------------------------------------------------------------------
        // Called by AcquiredPerkListController when the player makes a selection
        // -----------------------------------------------------------------------

        /// <summary>
        /// Assigns the perk with the given ID to this slot.
        /// Null/empty = remove; plays "denied" if restriction check fails.
        /// </summary>
        public void AssignPerk(string perkId)
        {
            var player = GameState.Player;
            if (player == null) return;

            // 1. Validate if the action is allowed
            if (string.IsNullOrEmpty(perkId))
            {
                var futurePerkIds = new List<string>(player.ActiveArenaPerkIds);
                if (!string.IsNullOrEmpty(_assignedPerkId)) futurePerkIds.Remove(_assignedPerkId);
                
                var futurePerks = futurePerkIds.Select(id => GameWorld.GetArenaPerkById(id)).ToList();
                if (player.Inventory.Slots.Count > Inventory.CalculateCapacity(futurePerks))
                {
                    Debug.LogWarning($"Cannot remove perk {_assignedPerkId} due to inventory bounds. Current perks: {string.Join(", ", player.ActiveArenaPerkIds)}");
                    PlayDenied();
                    return;
                }
            }
            else
            {
                var perkToActivate = GameWorld.GetArenaPerkById(perkId);
                if (!player.CanActivateArenaPerk(_assignedPerkId, perkToActivate))
                {
                    Debug.LogWarning($"Cannot activate perk {perkId}. Current perks: {string.Join(", ", player.ActiveArenaPerkIds)}");
                    PlayDenied();
                    return;
                }
            }

            // 2. Deactivate whatever is currently in this slot
            if (!string.IsNullOrEmpty(_assignedPerkId))
            {
                if (!player.TryDeactivateArenaPerk(_assignedPerkId))
                {
                    Debug.LogWarning($"Failed to deactivate currently assigned perk: {_assignedPerkId}");
                    return;
                }
            }

            // 3. Apply new state
            if (string.IsNullOrEmpty(perkId))
            {
                LoadEmpty();
            }
            else
            {
                if (!player.TryActivateArenaPerk(perkId))
                {
                    Debug.LogWarning($"Cannot activate perk {perkId}. Current perks: {string.Join(", ", player.ActiveArenaPerkIds)}");
                    PlayDenied();
                    
                    // Restore the previous perk if activation failed
                    if (!string.IsNullOrEmpty(_assignedPerkId))
                    {
                        player.TryActivateArenaPerk(_assignedPerkId);
                    }
                    return;
                }

                LoadPerk(GameWorld.GetArenaPerkById(perkId));
            }

            // 4. Finalize
            PlaySelectionMade();
            SaveGame();
        }

        // -----------------------------------------------------------------------
        // Hover and Select tooltip hookups
        // -----------------------------------------------------------------------

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && button.interactable)
                button.Select();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Optional: clear selection if mouse leaves, to clear the text.
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (perkDescriptionText != null)
                perkDescriptionText.text = _loadedPerk?.Description ?? "";
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (perkDescriptionText != null)
                perkDescriptionText.text = string.Empty;
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private void OnSlotClicked()
        {
            if (perkListPanel == null)
            {
                Debug.LogWarning("[PerkUI] No AcquiredPerkListController assigned to this slot.");
                return;
            }
            perkListPanel.Open(this);
        }

        private void PlayDenied()
        {
            if (audioSource == null) return;
            var clip = GameAssetRegistry.Instance.GetSound("denied");
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }

        private void PlaySelectionMade()
        {
            if (audioSource == null) return;
            var clip = GameAssetRegistry.Instance.GetSound("selection_made");
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }

        private void SaveGame()
        {
            if (GameState.SelectedSaveSlot != SaveSlots.Unknown)
                SaveSystem.SaveGame(GameState.SelectedSaveSlot);
        }
    }
}
