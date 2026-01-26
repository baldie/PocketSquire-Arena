using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PocketSquire.Arena.Core.LevelUp;
using PocketSquire.Arena.Core;
using DG.Tweening;
using TMPro;

namespace PocketSquire.Arena.Unity.UI.LevelUp
{
    public class LevelUpPresenter : MonoBehaviour
    {
        [SerializeField] private Button acceptButton;
        [SerializeField] private Transform levelUpBackground; // Parent containing rows and accept button
        [SerializeField] private TextMeshProUGUI pointsLabel;
        
        private ILevelUpModel _model;
        private List<AttributeRow> _attributeRows = new List<AttributeRow>();

        private void Start()
        {
        }

        // This method will be called to start the level up process with real data
        public void Initialize(Dictionary<string, int> currentAttributes, int availablePoints, int currentLevel)
        {
            Debug.Log("LevelUpPresenter.Initialize: availablePoints = " + availablePoints);
            _model = new LevelUpModel(currentAttributes, availablePoints, currentLevel);
            _model.OnStatsChanged += UpdateUI;

            if (pointsLabel == null && levelUpBackground != null)
            {
                var labelTransform = levelUpBackground.Find("PointsLabel");
                if (labelTransform != null)
                {
                    pointsLabel = labelTransform.GetComponent<TextMeshProUGUI>();
                }
            }

            InitializeRows();
            
            if (acceptButton != null)
            {
                acceptButton.onClick.AddListener(OnAcceptClicked);
            }

            UpdateUI();
        }

        private void InitializeRows()
        {
            _attributeRows.Clear();

            // Find all AttributeRow-capable objects under LevelUpBackground
            // Based on requirements, they are direct children named STR, CON, etc.
            // But we can just look for components to be safe and dynamic.
            foreach (Transform child in levelUpBackground)
            {
                // We check if the name matches our expected attribute keys or just grab all.
                // The user said: "STR, CON, INT, WIS, and LUC. Each of these are TextMeshPro gameobjects."
                // Since AttributeRow is a MonoBehaviour we attach to them, success depends on them having the script.
                // If they don't have it yet, we might need to add it or assume they do.
                // The task was to "Implement AttributeRow", implying we write the script.
                // The user's prompt implies we attach it.
                
                var row = child.GetComponent<AttributeRow>();
                if (row != null)
                {
                    row.Initialize();
                    
                    // Wire up events
                    string key = row.AttributeKey;
                    row.PlusButton.onClick.AddListener(() => OnIncrement(key));
                    row.MinusButton.onClick.AddListener(() => OnDecrement(key));

                    _attributeRows.Add(row);
                }
            }
        }

        private void OnIncrement(string key)
        {
            _model.IncrementAttribute(key);
        }

        private void OnDecrement(string key)
        {
            _model.DecrementAttribute(key);
        }

        private void OnAcceptClicked()
        {
            if (GameState.Player != null)
            {
                ApplyChangesToPlayer(GameState.Player);
                Debug.Log("Player stats updated.");
            }
            else
            {
                Debug.Log("No player to save to.");
            }

            LevelUpPresenter.HideLevelUpScreen(levelUpBackground as RectTransform);
        }

        private static Dictionary<string, int> PlayerAttributesToDictionary(Attributes attr)
        {
            return new Dictionary<string, int>
            {
                { "STR", attr.Strength },
                { "CON", attr.Constitution },
                { "INT", attr.Intelligence },
                { "WIS", attr.Wisdom },
                { "LUC", attr.Luck }
            };
        }

        private void ApplyChangesToPlayer(Player player)
        {
            player.AcceptNewLevel();

            // Map back from model to player attributes
            player.Attributes.Strength = _model.GetAttributeValue("STR");
            player.Attributes.Constitution = _model.GetAttributeValue("CON");
            player.Attributes.Intelligence = _model.GetAttributeValue("INT");
            player.Attributes.Wisdom = _model.GetAttributeValue("WIS");
            player.Attributes.Luck = _model.GetAttributeValue("LUC");
        }

        private void UpdateUI()
        {
            foreach (var row in _attributeRows)
            {
                int value = _model.GetAttributeValue(row.AttributeKey);
                int startingValue = _model.GetStartingAttributeValue(row.AttributeKey);
                
                bool canIncrement = _model.AvailablePoints > 0;
                bool canDecrement = value > startingValue;

                row.UpdateView(value, canIncrement, canDecrement);
            }

            if (pointsLabel != null)
            {
                pointsLabel.text = $"{_model.AvailablePoints}";
                // Use a premium blue (vibrant but not generic)
                pointsLabel.color = _model.AvailablePoints > 0 ? new Color(0.4f, 0.7f, 1.0f) : Color.white;
            }

            if (acceptButton != null)
            {
                acceptButton.interactable = _model.AvailablePoints == 0;
            }
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.OnStatsChanged -= UpdateUI;
            }
        }

        public static void Show(RectTransform levelUpBackground, LevelUpPresenter levelUpPresenter)
        {
            // Prepare the level up screen with the appropriate values
            int level = GameWorld.Progression.GetLevelForExperience(GameState.Player.Experience); 
            var reward = GameWorld.Progression.GetRewardForLevel(level);
            var attributes = PlayerAttributesToDictionary(GameState.Player.Attributes);
            int currentLevel = GameState.Player.Level;
            levelUpPresenter.Initialize(attributes, reward.StatPoints, currentLevel);

            // Animate the level up screen's appearance on the scene
            Sequence showSequence = DOTween.Sequence();
            showSequence.AppendInterval(1.0f);
            showSequence.Append(levelUpBackground.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutQuad));   
            Debug.Log("Level up screen shown");
        }

        private static void HideLevelUpScreen(RectTransform levelUpBackground)
        {
            // Calculate the off-screen position to the right
            float offScreenX = levelUpBackground.rect.width;
            Sequence hideSequence = DOTween.Sequence();
            hideSequence.AppendInterval(1.0f); 
            hideSequence.Append(levelUpBackground
                .DOAnchorPos(new Vector2(offScreenX, 0), 0.5f)
                .SetEase(Ease.InQuad));
        }
    }
}
