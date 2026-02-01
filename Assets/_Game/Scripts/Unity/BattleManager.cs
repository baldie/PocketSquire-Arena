using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Unity.UI;
using TMPro;
using DG.Tweening; // For animations
using UnityEngine.EventSystems;

namespace PocketSquire.Unity
{
    public class BattleManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject battleMenuUI;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button firstSelectedButton;
        [SerializeField] private ItemSelectionDialog itemSelectionDialog; // Added dialog reference
        
        [Header("Action Queue")]
        [Tooltip("Reference to the ActionQueueProcessor in the scene")]
        public ActionQueueProcessor actionQueueProcessor;

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

            attackButton = battleMenuUI.transform.Find("AttackButton")?.GetComponent<Button>();
            if (attackButton != null)
            {
                attackButton.onClick.RemoveAllListeners();
                attackButton.onClick.AddListener(Attack);
                if (firstSelectedButton == null) firstSelectedButton = attackButton;

                var sound = attackButton.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            var defendButton = battleMenuUI.transform.Find("DefendButton")?.GetComponent<Button>();
            if (defendButton != null)
            {
                defendButton.onClick.RemoveAllListeners();
                defendButton.onClick.AddListener(Defend);

                var sound = defendButton.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            var itemButton = battleMenuUI.transform.Find("ItemButton")?.GetComponent<Button>();
            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(Item);

                var sound = itemButton.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            var yieldButton = battleMenuUI.transform.Find("YieldButton")?.GetComponent<Button>();
            if (yieldButton != null)
            {
                yieldButton.onClick.RemoveAllListeners();
                yieldButton.onClick.AddListener(Yield);

                var sound = yieldButton.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }
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
