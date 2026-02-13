using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Unity.UI;

namespace PocketSquire.Unity.UI
{
    /// <summary>
    /// Controller for the Player Menu UI that displays character information and inventory.
    /// Reads data from GameState.Player and GameState.CurrentRun.
    /// </summary>
    public class PlayerMenuController : MonoBehaviour
    {
        [Header("Parent to Hide/Show")]
        [SerializeField] private GameObject menuParent;

        [Header("Character Info")]
        [SerializeField] private TextMeshProUGUI levelAndClassText;
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Images")]
        [SerializeField] private Image xpBarForeground;
        [SerializeField] private Image playerImage;


        [Header("Attributes")]
        [SerializeField] private TextMeshProUGUI strText;
        [SerializeField] private TextMeshProUGUI conText;
        [SerializeField] private TextMeshProUGUI intText;
        [SerializeField] private TextMeshProUGUI wisText;
        [SerializeField] private TextMeshProUGUI lckText;
        [SerializeField] private TextMeshProUGUI defText;

        [Header("Inventory")]
        [SerializeField] private CanvasGroup inventoryCanvasGroup;
        [SerializeField] private Transform inventoryScrollContent;

        [Header("Containers")]
        [SerializeField] private Transform badgesContainer;

        [Header("Footer")]
        [SerializeField] private Button skillTreeButton;

        [Header("Cursor Settings")]
        [SerializeField] private Vector3 inventoryCursorOffset = new Vector3(-60f, 0, 0);


        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        private bool isOpen = false;

        public bool IsOpen => isOpen;
        
        // State tracking for background UI disabling
        private class CanvasState
        {
            public CanvasGroup group;
            public bool wasInteractable;
            public bool wasBlocking;
        }
        private System.Collections.Generic.List<CanvasState> _disabledCanvases = new System.Collections.Generic.List<CanvasState>();

        private void Awake()
        {
            if (inventoryScrollContent == null)
            {
                var contentTransform = transform.Find("BodyContainer/RightColumn/InventoryScrollView/Viewport/Content");
                if (contentTransform != null)
                {
                    inventoryScrollContent = contentTransform;
                }
            }

            // Ensure the Mask component on the Viewport is disabled
            if (inventoryScrollContent != null && inventoryScrollContent.parent != null)
            {
                var mask = inventoryScrollContent.parent.GetComponent<Mask>();
                if (mask != null)
                {
                    mask.enabled = false;
                }
            }

            this.Close();
        }

        private void Start()
        {
            // Wire up footer button
            if (skillTreeButton != null)
            {
                skillTreeButton.onClick.RemoveAllListeners();
                skillTreeButton.onClick.AddListener(OnSkillTreeButtonClicked);

                // Add MenuCursorTarget to SkillTreeButton if missing (or we can do this in Editor)
                // But for safety/completeness based on plan:
                var target = skillTreeButton.GetComponent<MenuCursorTarget>();
                if (target == null) target = skillTreeButton.gameObject.AddComponent<MenuCursorTarget>();
                // Value from Plan: (-60, 40.5, 0). 
                if (target.cursorOffset == Vector3.zero) target.cursorOffset = new Vector3(-60f, 40.5f, 0f);
            }
                
            // Initial refresh and ensure closed
            Refresh();
            Close();
        }

        private void Update()
        {
            if (InputManager.GetButtonDown("Inventory"))
            {
                if (isOpen) 
                {
                    InputManager.ConsumeButton("Pause");
                    Close();
                }
                else Open();
            }
            else if (isOpen && InputManager.GetButtonDown("Cancel"))
            {
                InputManager.ConsumeButton("Pause");
                InputManager.ConsumeButton("Cancel");
                Close();
            }
        }

        /// <summary>
        /// Refreshes all UI elements from GameState.Player and GameState.CurrentRun
        /// </summary>
        public void Refresh()
        {
            var player = GameState.Player;
            if (player == null)
            {
                return;
            }
            
            // Re-bind references if needed (since it's a prefab instance)
            // But they should already be wired in the prefab.
            
            // Update character info
            if (levelAndClassText != null)
            {
                // TODO: Add class property to Player when implemented
                levelAndClassText.text = $"Level {player.Level} {player.Class}";
            }


            if (goldText != null)
            {
                goldText.text = player.Gold.ToString();
            }

            // Update experience bar
            UpdateExperienceBar(player);

            // Update attributes
            UpdateAttributes(player);

            // Update player image
            UpdatePlayerImage(player);

            // Update inventory
            UpdateInventory(player);
        }

        /// <summary>
        /// Updates the player sprite image from the registry
        /// </summary>
        private void UpdatePlayerImage(Player player)
        {
            if (playerImage == null) return;
            Sprite playerSprite = GameAssetRegistry.Instance.GetSprite(player.SpriteId);
            if (playerSprite != null)
            {
                playerImage.sprite = playerSprite;
            }
        }

        /// <summary>
        /// Updates the experience bar fill amount based on current and required XP
        /// </summary>
        private void UpdateExperienceBar(Player player)
        {
            if (xpBarForeground == null) return;

            if (GameWorld.Progression == null)
            {
                xpBarForeground.fillAmount = 0f;
                return;
            }

            // Get XP required for current level and next level
            int currentLevel = player.Level;
            int currentXp = player.Experience;
            
            // Get XP thresholds from level rewards
            var currentLevelReward = GameWorld.Progression.GetRewardForLevel(currentLevel);
            var nextLevelReward = GameWorld.Progression.GetRewardForLevel(currentLevel + 1);
            
            int xpForCurrentLevel = currentLevelReward.ExperienceRequired;
            int xpForNextLevel = nextLevelReward.ExperienceRequired;
            
            // Calculate fill amount (progress within current level)
            int xpIntoCurrentLevel = currentXp - xpForCurrentLevel;
            int xpRequiredForLevel = xpForNextLevel - xpForCurrentLevel;
            
            float fillAmount = xpRequiredForLevel > 0 
                ? (float)xpIntoCurrentLevel / xpRequiredForLevel 
                : 0f;
            
            xpBarForeground.fillAmount = Mathf.Clamp01(fillAmount);
        }

        /// <summary>
        /// Updates all attribute text fields from player's Attributes
        /// </summary>
        private void UpdateAttributes(Player player)
        {
            if (player.Attributes == null) return;

            if (strText != null) strText.text = $"STR: {player.Attributes.Strength}";
            if (conText != null) conText.text = $"CON: {player.Attributes.Constitution}";
            if (intText != null) intText.text = $"INT: {player.Attributes.Intelligence}";
            if (wisText != null) wisText.text = $"WIS: {player.Attributes.Wisdom}";
            if (lckText != null) lckText.text = $"LCK: {player.Attributes.Luck}";
            if (defText != null) defText.text = $"DEF: {player.Attributes.Defense}";
        }

        private void UpdateInventory(Player player)
        {
            var prefab = GameAssetRegistry.Instance.itemRowPrefab;
            if (inventoryScrollContent == null || prefab == null) return;
            // Clear existing
            foreach (Transform child in inventoryScrollContent)
            {
                Destroy(child.gameObject);
            }

            if (player.Inventory == null) return;

            // Populate new
            foreach (var slot in player.Inventory.Slots)
            {
                var item = GameWorld.GetItemById(slot.ItemId);
                if (item == null) continue;

                if (slot.Quantity <= 0) continue;

                var go = Instantiate(prefab, inventoryScrollContent);
                go.SetActive(true);

                var itemRow = go.GetComponent<ItemRow>();

                // Hook up the audio source for the item row
                var menuButtonSound = go.GetComponent<MenuButtonSound>();
                if (menuButtonSound != null && audioSource != null)
                {
                    menuButtonSound.source = audioSource;
                }
                
                // Ensure scale is correct (sometimes instantiation in layout groups gets wonky)
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = Vector3.zero;

                // Configure Menu Cursor Target for this item
                var cursorTarget = go.GetComponent<MenuCursorTarget>();
                if (cursorTarget == null) cursorTarget = go.AddComponent<MenuCursorTarget>();
                cursorTarget.cursorOffset = inventoryCursorOffset;
                cursorTarget.useLocalOffset = true;

                // Ensure LayoutElement exists for correct sizing in ScrollRect
                var layoutElement = go.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = go.AddComponent<LayoutElement>();
                    layoutElement.minHeight = 100f; // Matching prefab height
                    layoutElement.preferredHeight = 100f;
                    layoutElement.flexibleWidth = 1f;
                }
                
                if (itemRow != null)
                {
                    Sprite icon = null;
                    if (!string.IsNullOrEmpty(item.Sprite))
                    {
                        icon = GameAssetRegistry.Instance.GetSprite(item.Sprite);
                    }

                    itemRow.Initialize(item, slot.Quantity, icon, () => {
                        // Action on click
                    }, showPrice: false);
                }
            }
            
            // Force layout rebuild (sometimes needed for ScrollRects appearing for first time)
            LayoutRebuilder.ForceRebuildLayoutImmediate(inventoryScrollContent as RectTransform);
        }

        /// <summary>
        /// Toggles whether the inventory is interactable (for read-only mode)
        /// </summary>
        public void SetInventoryInteractable(bool interactable)
        {
            if (inventoryCanvasGroup != null)
            {
                inventoryCanvasGroup.interactable = interactable;
            }
        }

        /// <summary>
        /// Called when the footer button (Skill Tree) is clicked
        /// </summary>
        private void OnSkillTreeButtonClicked()
        {
        }

        public void Open()
        {
            if (isOpen) return;

            if (menuParent != null)
            {
                menuParent.SetActive(true);
            }
            
            isOpen = true;
            Refresh();
            
            // Select the first available item so navigation works immediately
            SelectFirstInteractable();

            Time.timeScale = 0f;
        }

        private void SelectFirstInteractable()
        {
            // 1. Try to select the first item in the inventory
            if (inventoryScrollContent.childCount > 0)
            {
                var firstItem = inventoryScrollContent.GetChild(0).gameObject;
                if (firstItem.activeInHierarchy)
                {
                    EventSystem.current.SetSelectedGameObject(firstItem);
                    return;
                }
            }

            // 2. Fallback to Skill Tree Button if inventory is empty
            if (skillTreeButton != null && skillTreeButton.interactable)
            {
                EventSystem.current.SetSelectedGameObject(skillTreeButton.gameObject);
                return;
            }
        }

        public void Close()
        {
            if (!isOpen) return;

            if (menuParent != null)
            {
                menuParent.SetActive(false);
            }
                
            isOpen = false;
            
            // Clear selection when closing to prevent ghost inputs
            EventSystem.current.SetSelectedGameObject(null);
            
            // Unpause
            Time.timeScale = 1f;
        }
    }
}
