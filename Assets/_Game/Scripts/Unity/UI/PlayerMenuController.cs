using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        [Header("Character Info")]
        [SerializeField] private TextMeshProUGUI levelAndClassText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Image playerImage;

        [Header("Experience Bar")]
        [SerializeField] private Image xpBarForeground;

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

        [Header("Assets")]
        [SerializeField] private GameObject itemRowPrefab;
        [SerializeField] private GameAssetRegistry gameAssetRegistry;

        private bool isOpen = false;

        public bool IsOpen => isOpen;
        
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            
            if (inventoryScrollContent == null)
            {
                var contentTransform = transform.Find("BodyContainer/RightColumn/InventoryScrollView/Viewport/Content");
                if (contentTransform != null)
                {
                    inventoryScrollContent = contentTransform;
                }
            }

            if (playerImage == null)
            {
                var imageTransform = transform.Find("BodyContainer/LeftColumn/MidPanel/PlayerImage");
                if (imageTransform != null)
                {
                    playerImage = imageTransform.GetComponent<Image>();
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
        }

        private void Start()
        {
            // Wire up footer button
            if (skillTreeButton != null)
            {
                skillTreeButton.onClick.RemoveAllListeners();
                skillTreeButton.onClick.AddListener(OnSkillTreeButtonClicked);
            }
                
            // Initial refresh and ensure closed
            Refresh();
            Close();
        }

        private void Update()
        {
            if (InputManager.GetButtonDown("Inventory"))
            {
                if (isOpen) Close();
                else Open();
            }
            else if (isOpen && InputManager.GetButtonDown("Cancel"))
            {
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
            if (playerImage == null || gameAssetRegistry == null) return;

            // Use the battle sprite as the default menu representation or 
            // consider context-specific sprites if available
            string spriteId = player.SpriteId;
            
            Sprite playerSprite = gameAssetRegistry.GetSprite(spriteId);
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
            if (inventoryScrollContent == null) return;
            if (itemRowPrefab == null)
            {
                return;
            }
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

                var go = Instantiate(itemRowPrefab, inventoryScrollContent);
                go.SetActive(true);
                
                // Ensure scale is correct (sometimes instantiation in layout groups gets wonky)
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = Vector3.zero;

                // Ensure LayoutElement exists for correct sizing in ScrollRect
                var layoutElement = go.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = go.AddComponent<LayoutElement>();
                    layoutElement.minHeight = 100f; // Matching prefab height
                    layoutElement.preferredHeight = 100f;
                    layoutElement.flexibleWidth = 1f;
                }

                var row = go.GetComponent<ItemRow>();
                
                if (row != null)
                {
                    Sprite icon = null;
                    if (gameAssetRegistry != null && !string.IsNullOrEmpty(item.Sprite))
                    {
                        icon = gameAssetRegistry.GetSprite(item.Sprite);
                    }

                    row.Initialize(item, slot.Quantity, icon, () => {
                        // Action on click
                    });
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
            if (_canvas != null)
                _canvas.enabled = true;
            
            isOpen = true;
            Refresh();
            
            // Optional: Pause game if in Arena
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Arena")
            {
                Time.timeScale = 0f;
            }
        }

        public void Close()
        {
            if (_canvas != null)
                _canvas.enabled = false;
                
            isOpen = false;
            
            // Unpause
            Time.timeScale = 1f;
        }
    }
}
