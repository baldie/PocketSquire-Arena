using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core;

namespace PocketSquire.Unity
{
    public class BattleManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject battleMenuUI;
        public Button firstSelectedButton;
        
        [Header("Action Queue")]
        [Tooltip("Reference to the ActionQueueProcessor in the scene")]
        public ActionQueueProcessor actionQueueProcessor;



        void Start()
        {
            if (actionQueueProcessor == null)
            {
                Debug.LogError("BattleManager: ActionQueueProcessor not found");
            }
            WireButtons();
        }

        private void WireButtons()
        {
            var uiAudio = GameObject.Find("UIAudio");
            var audioSource = uiAudio != null ? uiAudio.GetComponent<AudioSource>() : null;

            var attackButton = battleMenuUI.transform.Find("AttackButton")?.GetComponent<Button>();
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

        public void Attack()
        {
            Debug.Log("Attack");

            if (actionQueueProcessor != null && GameWorld.Battle != null)
            {
                var player = GameState.Player;
                
                if (player != null)
                {
                    var attackAction = new AttackAction(player, GameWorld.Battle.CurrentTurn.Target);
                    actionQueueProcessor.EnqueueAction(attackAction);
                }
            }
        }

        public void Defend()
        {
            Debug.Log("Defend");

            if (actionQueueProcessor != null && GameWorld.Battle != null)
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
            
            if (actionQueueProcessor != null && GameWorld.Battle != null)
            {
                var player = GameState.Player;
                
                if (player != null)
                {
                    var itemAction = new ItemAction();
                    actionQueueProcessor.EnqueueAction(itemAction);
                }
            }
        }

        public void Yield()
        {
            Debug.Log("Yield");
            
            if (actionQueueProcessor != null && GameWorld.Battle != null)
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
