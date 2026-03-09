using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Perks;

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

        // Optional: shown while hovering — can be wired from PlayerMenuController.
        public TextMeshProUGUI perkDescriptionText;



        private ArenaPerk _loadedPerk;
        private string _assignedPerkId; // null/empty = empty slot

        public bool HasAssignedPerk => !string.IsNullOrEmpty(_assignedPerkId);

        private AudioSource _audioSource;

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

            // Find audio source — self, parent, or a known UI audio GO.
            _audioSource = GetComponent<AudioSource>()
                ?? GetComponentInParent<AudioSource>()
                ?? GameObject.Find("UIAudio")?.GetComponent<AudioSource>();

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
                    perkIcon.color = Color.white;
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
            perkIcon.color = Color.white; // Not dimmed — slot is open but still interactive.
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
                perkIcon.color = Color.white;
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

            // 1. Deactivate whatever is currently in this slot.
            if (!string.IsNullOrEmpty(_assignedPerkId))
                player.TryDeactivateArenaPerk(_assignedPerkId);

            // 2. Remove path.
            if (string.IsNullOrEmpty(perkId))
            {
                LoadEmpty();
                SaveGame();
                return;
            }

            // 3. TryActivateArenaPerk enforces slot cap and will later enforce perk restrictions.
            bool activated = player.TryActivateArenaPerk(perkId);
            if (!activated)
            {
                PlayDenied();
                // Restore the perk that was here before.
                if (!string.IsNullOrEmpty(_assignedPerkId))
                    player.TryActivateArenaPerk(_assignedPerkId);
                return;
            }

            var perk = GameWorld.GetArenaPerkById(perkId);
            LoadPerk(perk);
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
            if (_audioSource == null) return;
            var clip = GameAssetRegistry.Instance.GetSound("denied");
            if (clip != null)
                _audioSource.PlayOneShot(clip);
        }

        private void SaveGame()
        {
            if (GameState.SelectedSaveSlot != SaveSlots.Unknown)
                SaveSystem.SaveGame(GameState.SelectedSaveSlot);
        }
    }
}
