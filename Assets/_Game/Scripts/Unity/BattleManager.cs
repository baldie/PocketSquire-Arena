using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PocketSquire.Arena.Core;

namespace PocketSquire.Unity
{
    public class BattleManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject battleMenuUI;
        public Button firstSelectedButton;
        public Image playerHealthBar;
        public Image enemyHealthBar;
        
        [Header("Action Queue")]
        [Tooltip("Reference to the ActionQueueProcessor in the scene")]
        public ActionQueueProcessor actionQueueProcessor;

        void Update()
        {

        }

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
                var actor = GameWorld.Battle.CurrentTurn.Actor;
                var target = GameWorld.Battle.CurrentTurn.Target;
                
                if (actor != null && target != null)
                {
                    int damage = CalculateDamage(actor, target);
                    var healthbar = target is Player ? playerHealthBar : enemyHealthBar;
                    var attackAction = new AttackAction(actor, target, damage, () => {
                        healthbar.fillAmount = (float)target.Health / target.MaxHealth;
                    });
                    actionQueueProcessor.EnqueueAction(attackAction);               
                    Debug.Log("Health: " + target.Health + " / " + target.MaxHealth + " = " + healthbar.fillAmount);
                }
            }
            else
            {
                // Fallback if no processor
                GameWorld.Battle?.CurrentTurn?.End();
            }
        }
        
        private int CalculateDamage(Entity attacker, Entity target)
        {
            // Basic damage calculation - can be made more complex later
            int baseDamage = attacker.Attributes.Strength;
            return System.Math.Max(1, baseDamage); // Minimum 1 damage
        }

        public void Defend()
        {
            Debug.Log("Defend");

            if (actionQueueProcessor != null && GameWorld.Battle != null)
            {
                var player = GameState.Player;
                
                if (player != null)
                {
                    var blockAction = new BlockAction(player);
                    actionQueueProcessor.EnqueueAction(blockAction);
                }
            }
            else
            {
                // Fallback
                GameWorld.Battle?.CurrentTurn?.End();
            }
        }

        public void Item()
        {
            Debug.Log("Item");
            GameWorld.Battle.CurrentTurn.End();
        }

        public void Yield()
        {
            Debug.Log("Yield");
            GameWorld.Battle.CurrentTurn.End();
        }
    }
}
