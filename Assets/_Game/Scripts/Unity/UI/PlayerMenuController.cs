using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PocketSquire.Arena.Core;

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

        [Header("Experience Bar")]
        [SerializeField] private Image xpBarBackground;
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

        private bool isOpen = false;

        public bool IsOpen => isOpen;
        
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
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

            // TODO: Update inventory, badges, and status effects when those systems are ready
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
            Debug.Log("Skill Tree clicked");
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
