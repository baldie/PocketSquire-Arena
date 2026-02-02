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
        [SerializeField] private TextMeshProUGUI classText;
        [SerializeField] private TextMeshProUGUI levelText;
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

        [Header("Inventory")]
        [SerializeField] private CanvasGroup inventoryCanvasGroup;
        [SerializeField] private Transform inventoryScrollContent;

        [Header("Containers")]
        [SerializeField] private Transform badgesContainer;
        [SerializeField] private Transform inventoryGrid;
        [SerializeField] private Transform statusEffectsRow;

        [Header("Footer")]
        [SerializeField] private Button footerButton;

        private void Start()
        {
            // Wire up footer button
            if (footerButton != null)
            {
                footerButton.onClick.RemoveAllListeners();
                footerButton.onClick.AddListener(OnFooterButtonClicked);
            }

            // Initial refresh
            Refresh();
        }

        /// <summary>
        /// Refreshes all UI elements from GameState.Player and GameState.CurrentRun
        /// </summary>
        public void Refresh()
        {
            var player = GameState.Player;
            if (player == null)
            {
                Debug.LogWarning("PlayerMenuController: GameState.Player is null");
                return;
            }

            // Update character info
            if (classText != null)
            {
                // TODO: Add class property to Player when implemented
                classText.text = "Class: Adventurer";
            }

            if (levelText != null)
            {
                levelText.text = $"Level: {player.Level}";
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

            if (strText != null)
                strText.text = $"STR: {player.Attributes.Strength}";

            if (conText != null)
                conText.text = $"CON: {player.Attributes.Constitution}";

            if (intText != null)
                intText.text = $"INT: {player.Attributes.Intelligence}";

            if (wisText != null)
                wisText.text = $"WIS: {player.Attributes.Wisdom}";

            if (lckText != null)
                lckText.text = $"LCK: {player.Attributes.Luck}";
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
        private void OnFooterButtonClicked()
        {
            Debug.Log("Skill Tree clicked");
        }
    }
}
