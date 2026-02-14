using System;
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
        [SerializeField] private GameObject levelUpBackground;
        [SerializeField] private TextMeshProUGUI pointsLabel;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip levelUpSound;
        [SerializeField] private AudioClip statChangeSound;
        [SerializeField] private AudioClip acceptSound;
        
        private ILevelUpModel _model;
        private List<AttributeRow> _attributeRows = new List<AttributeRow>();
        private Action _onAccept;

        private void Start()
        {
        }

        // This method will be called to start the level up process with real data
        public void Initialize(Dictionary<string, int> currentAttributes, int availablePoints, int currentLevel)
        {
            _model = new LevelUpModel(currentAttributes, availablePoints, currentLevel);
            _model.OnStatsChanged += UpdateUI;

            if (pointsLabel == null && levelUpBackground != null)
            {
                var labelTransform = levelUpBackground.transform.Find("PointsLabel");
                if (labelTransform != null)
                {
                    pointsLabel = labelTransform.GetComponent<TextMeshProUGUI>();
                }
            }

            InitializeRows();
            
            if (acceptButton != null)
            {
                acceptButton.onClick.RemoveListener(OnAcceptClicked);
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
            foreach (Transform child in levelUpBackground.transform)
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
                    row.PlusButton.onClick.RemoveAllListeners();
                    row.PlusButton.onClick.AddListener(() => OnIncrement(key));
                    row.MinusButton.onClick.RemoveAllListeners();
                    row.MinusButton.onClick.AddListener(() => OnDecrement(key));

                    _attributeRows.Add(row);
                }
            }
        }

        private void OnIncrement(string key)
        {
            _model.IncrementAttribute(key);

            if (audioSource != null && statChangeSound != null)
            {
                audioSource.PlayOneShot(statChangeSound);
            }
        }

        private void OnDecrement(string key)
        {
            _model.DecrementAttribute(key);

            if (audioSource != null && statChangeSound != null)
            {
                audioSource.PlayOneShot(statChangeSound);
            }
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

            if (audioSource != null && acceptSound != null)
            {
                audioSource.PlayOneShot(acceptSound);
            }

            LevelUpPresenter.HideLevelUpScreen(levelUpBackground);
            
            // Wait half a sec and then show the arena menu
            DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() => {
                _onAccept?.Invoke();
                _onAccept = null;
            });
        }

        private static Dictionary<string, int> PlayerAttributesToDictionary(Attributes attr)
        {
            return new Dictionary<string, int>
            {
                { "STR", attr.Strength },
                { "CON", attr.Constitution },
                { "INT", attr.Intelligence },
                { "AGI", attr.Agility },
                { "LUC", attr.Luck },
                { "DEF", attr.Defense }
            };
        }

        private void ApplyChangesToPlayer(Player player)
        {
            player.AcceptNewLevel();

            // Map back from model to player attributes
            player.Attributes.Strength = _model.GetAttributeValue("STR");
            player.Attributes.Constitution = _model.GetAttributeValue("CON");
            player.Attributes.Intelligence = _model.GetAttributeValue("INT");
            player.Attributes.Agility = _model.GetAttributeValue("AGI");
            player.Attributes.Luck = _model.GetAttributeValue("LUC");
            player.Attributes.Defense = _model.GetAttributeValue("DEF");
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

        public static void Show(GameObject levelUpBackground, LevelUpPresenter levelUpPresenter, Action onAccept)
        {
            levelUpPresenter._onAccept = onAccept;
            Debug.Log("Showing level up screen");
            levelUpBackground.SetActive(true);

            // Prepare the level up screen with the appropriate values
            int level = GameWorld.Progression.GetLevelForExperience(GameState.Player.Experience); 
            var reward = GameWorld.Progression.GetRewardForLevel(level);
            var attributes = PlayerAttributesToDictionary(GameState.Player.Attributes);
            int currentLevel = GameState.Player.Level;
            levelUpPresenter.Initialize(attributes, reward.StatPoints, currentLevel);

            // --- PERK SELECTION LOGIC ---
            var perkChoices = new List<string>();
            // 1. Fixed Perks (Legacy)
            if (reward.FixedPerkIds != null)
            {
                perkChoices.AddRange(reward.FixedPerkIds);
            }

            // 2. Dynamic Pool Perks
            if (!string.IsNullOrEmpty(reward.PerkPoolTag) && GameWorld.PerkPools.TryGetValue(reward.PerkPoolTag, out var pool))
            {
                var context = new PerkSelector.SelectionContext
                {
                    PlayerLevel = level,
                    UnlockedPerkIds = GameState.Player.UnlockedPerks
                };
                
                // Use default Random for now (or improve with seeded logic later)
                var rng = new System.Random(); 
                
                var dynamicPerks = PerkSelector.Select(pool, reward.PerkPoolDrawCount, context, rng);
                foreach(var p in dynamicPerks)
                {
                    perkChoices.Add(p.Id);
                }
            }
            
            // 3. Push to Model
            if (perkChoices.Count > 0)
            {
                levelUpPresenter._model.SetPendingPerkChoices(perkChoices);
                Debug.Log($"[LevelUp] Generated {perkChoices.Count} perk choices: {string.Join(", ", perkChoices)}");
            }

            if (levelUpPresenter.audioSource != null && levelUpPresenter.levelUpSound != null)
            {
                levelUpPresenter.audioSource.PlayOneShot(levelUpPresenter.levelUpSound);
            }

            // Animate the level up screen's appearance on the scene
            RectTransform rectTransform = levelUpBackground.GetComponent<RectTransform>();
            Sequence showSequence = DOTween.Sequence();
            showSequence.AppendInterval(1.0f);
            showSequence.Append(rectTransform.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutQuad));   
        }

        private static void HideLevelUpScreen(GameObject levelUpBackground)
        {
            // Calculate the off-screen position to the right
            RectTransform rectTransform = levelUpBackground.GetComponent<RectTransform>();
            float offScreenX = rectTransform.rect.width;
            Sequence hideSequence = DOTween.Sequence();
            hideSequence.AppendInterval(1.0f); 
            hideSequence.Append(rectTransform.DOAnchorPos(new Vector2(offScreenX, 0), 0.5f)
                .SetEase(Ease.InQuad));
            hideSequence.AppendInterval(1.0f);
            hideSequence.AppendCallback(() => levelUpBackground.SetActive(false));
        }
    }
}
