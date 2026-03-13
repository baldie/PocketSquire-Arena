using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Unity.UI;
using TMPro;
using DG.Tweening; // For animations
using UnityEngine.EventSystems;
using System.Linq;

namespace PocketSquire.Unity
{
    public class BattleManager : MonoBehaviour
    {
        private static readonly Color SpecialAvailableColor = Color.white;
        private static readonly Color SpecialUnavailableColor = new Color(0.55f, 0.7f, 1f, 1f);

        [Header("UI References")]
        [SerializeField] private GameObject battleMenuUI;
        [SerializeField] private Button specialButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button defendButton;
        [SerializeField] private Button itemButton;
        [SerializeField] private Button yieldButton;
        [SerializeField] private Button firstSelectedButton;
        [SerializeField] private ItemSelectionDialog itemSelectionDialog; // Added dialog reference
        
        [Header("Action Queue")]
        [Tooltip("Reference to the ActionQueueProcessor in the scene")]
        public ActionQueueProcessor actionQueueProcessor;

        private const int HEIGHT_WITH_SPECIAL_ATTACK = 580;
        private const int HEIGHT_WITHOUT_SPECIAL_ATTACK = 478;

        void Start()
        {
            if (actionQueueProcessor == null)
            {
                Debug.LogError("BattleManager: ActionQueueProcessor not found");
            }
            else
            {
                // Subscribe to know when to show the menu again
                actionQueueProcessor.OnProcessingFinished += ShowMenu;
            }

            if (itemSelectionDialog == null)
            {
                itemSelectionDialog = FindFirstObjectByType<ItemSelectionDialog>();
                if (itemSelectionDialog != null)
                {
                    itemSelectionDialog.gameObject.SetActive(false); // Ensure hidden at start
                }
            }

            WireButtons();
            
            // Explicitly show menu at start if it's player turn
            ShowMenu();
        }

        private void WireButtons()
        {
            var uiAudio = GameObject.Find("UIAudio");
            var audioSource = uiAudio != null ? uiAudio.GetComponent<AudioSource>() : null;

            var rectTransform = battleMenuUI.GetComponent<RectTransform>();
            if (GameState.Player?.ActivePerks?.Any(p => p.Id == "special_attack") ?? false)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, HEIGHT_WITH_SPECIAL_ATTACK);
                specialButton.gameObject.SetActive(true);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, HEIGHT_WITHOUT_SPECIAL_ATTACK);
                specialButton.gameObject.SetActive(false);
            }

            if (specialButton != null)
            {
                specialButton.onClick.RemoveAllListeners();
                specialButton.onClick.AddListener(Special);
                if (firstSelectedButton == null) firstSelectedButton = specialButton;

                var sound = specialButton.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            if (attackButton != null)
            {
                attackButton.onClick.RemoveAllListeners();
                attackButton.onClick.AddListener(Attack);
                if (firstSelectedButton == null) firstSelectedButton = attackButton;

                var sound = attackButton.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            if (defendButton != null)
            {
                defendButton.onClick.RemoveAllListeners();
                defendButton.onClick.AddListener(Defend);

                var sound = defendButton.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(Item);

                var sound = itemButton.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            if (yieldButton != null)
            {
                yieldButton.onClick.RemoveAllListeners();
                yieldButton.onClick.AddListener(Yield);

                var sound = yieldButton.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            RefreshSpecialButtonState();
        }

        public void ShowMenu()
        {
            if (battleMenuUI == null) return;

            // Only show if it's the player's turn and no actions are currently processing
            bool isPlayerTurn = GameState.Battle?.CurrentTurn?.IsPlayerTurn ?? false;
            bool isProcessing = actionQueueProcessor?.IsProcessing ?? false;

            if (isPlayerTurn && !isProcessing)
            {
                battleMenuUI.SetActive(true);
                RefreshSpecialButtonState();
                if (firstSelectedButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
                }
            }
            else
            {
                battleMenuUI.SetActive(false);
            }
        }

        public void HideMenu()
        {
            if (battleMenuUI != null) battleMenuUI.SetActive(false);
        }

        public void Special()
        {
            Debug.Log("Special");

            if (GameState.Player != null && !GameState.Player.CanAffordSpecialAttack())
            {
                Debug.LogWarning("Special attack attempted without sufficient mana — this should not be reachable.");
                ShowMenu();
                return;
            }

            HideMenu();

            if (actionQueueProcessor != null && GameState.Battle != null)
            {
                var player = GameState.Player;
                
                if (player != null)
                {
                    var specialAttackAction = new SpecialAttackAction(player, GameState.Battle.CurrentTurn.Target);
                    // Wait 0.1 seconds before enqueuing the action to allow the menu selection sfx to play
                    DOTween.Sequence().AppendInterval(0.4f).AppendCallback(() => actionQueueProcessor.EnqueueAction(specialAttackAction));
                }
            }
        }

        private void RefreshSpecialButtonState()
        {
            if (specialButton == null || battleMenuUI == null)
            {
                return;
            }

            var player = GameState.Player;
            bool hasSpecialAttackPerk = player?.ActivePerks?.Any(p => p.Id == "special_attack") ?? false;

            var rectTransform = battleMenuUI.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(
                    rectTransform.sizeDelta.x,
                    hasSpecialAttackPerk ? HEIGHT_WITH_SPECIAL_ATTACK : HEIGHT_WITHOUT_SPECIAL_ATTACK);
            }

            specialButton.gameObject.SetActive(hasSpecialAttackPerk);
            if (!hasSpecialAttackPerk || player == null)
            {
                return;
            }

            bool canUseSpecial = player.CanAffordSpecialAttack();
            specialButton.interactable = canUseSpecial;

            var buttonImage = specialButton.image;
            if (buttonImage != null)
            {
                buttonImage.color = canUseSpecial ? SpecialAvailableColor : SpecialUnavailableColor;
            }
        }

        public void Attack()
        {
            Debug.Log("Attack");
            HideMenu();

            if (actionQueueProcessor != null && GameState.Battle != null)
            {
                var player = GameState.Player;
                
                if (player != null)
                {
                    var attackAction = new AttackAction(player, GameState.Battle.CurrentTurn.Target);
                    // Wait 0.1 seconds before enqueuing the action to allow the menu selection sfx to play
                    DOTween.Sequence().AppendInterval(0.4f).AppendCallback(() => actionQueueProcessor.EnqueueAction(attackAction));
                }
            }
        }

        public void Defend()
        {
            Debug.Log("Defend");
            HideMenu();

            if (actionQueueProcessor != null && GameState.Battle != null)
            {
                var player = GameState.Player;
                
                if (player != null)
                {
                    var defendAction = new DefendAction(player);
                    actionQueueProcessor.EnqueueAction(defendAction);
                }
            }
        }

        public void Item()
        {
            Debug.Log("Item");
            
            if (GameState.Player == null || GameState.Battle == null)
            {
                Debug.LogWarning("Cannot use item: No player or battle active");
                return;
            }

            // Hide battle menu while dialog is shown
            HideMenu();

            ItemSelectionDialog.Show(
                itemSelectionDialog,
                onItemSelected: (itemId) => 
                {
                    // Create ItemAction with the selected item
                    var itemAction = new ItemAction(itemId);
                    
                    // Delay to allow selection sound to play
                    DOTween.Sequence()
                        .AppendInterval(0.4f)
                        .AppendCallback(() => 
                        {
                            if (actionQueueProcessor != null)
                                actionQueueProcessor.EnqueueAction(itemAction);
                            
                            // Menu will be re-shown by OnProcessingFinished event
                        });
                },
                onCancel: () => 
                {
                    // User cancelled - return to their turn
                    ShowMenu();
                }
            );
        }

        public void Yield()
        {
            Debug.Log("Yield");
            HideMenu();
            
            if (actionQueueProcessor != null && GameState.Battle != null)
            {
                var player = GameState.Player;
                
                if (player != null)
                {
                    var yieldAction = new YieldAction();
                    actionQueueProcessor.EnqueueAction(yieldAction);
                }
            }
        }
    }
}
