using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PocketSquire.Arena.Core;
using DG.Tweening;

namespace PocketSquire.Unity
{
    public class BattleManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject battleMenuUI;
        public Button firstSelectedButton;
        public Image playerHealthBarActual;
        public Image playerHealthBarGhost;
        public Image monsterHealthBarActual;
        public Image monsterHealthBarGhost;
        
        [Header("Action Queue")]
        [Tooltip("Reference to the ActionQueueProcessor in the scene")]
        public ActionQueueProcessor actionQueueProcessor;

        private enum ShakeType { None, Hit, Heal }

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
            if (actionQueueProcessor != null && GameWorld.Battle != null)
            {
                var actor = GameWorld.Battle.CurrentTurn.Actor;
                var target = GameWorld.Battle.CurrentTurn.Target;
                
                if (actor != null && target != null)
                {
                    int damage = CalculateDamage(actor, target);
                    var healthbarActual = target is Player ? playerHealthBarActual : monsterHealthBarActual;
                    var healthbarGhost = target is Player ? playerHealthBarGhost : monsterHealthBarGhost;
                    var attackAction = new AttackAction(actor, target, damage, () => {
                        UpdateHealth(healthbarActual, healthbarGhost, target.Health, target.MaxHealth, ShakeType.Hit);
                    });
                    actionQueueProcessor.EnqueueAction(attackAction);
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

        private void UpdateHealth(Image healthBarActual, Image healthBarGhost, int currentHealth, int maxHealth, ShakeType shakeType)
        {
            float targetFill = (float)currentHealth / maxHealth;

            // 1. Shake health bar
            if (shakeType == ShakeType.Hit)
            {
                healthBarActual.transform.parent.DOKill(true); // Complete previous shake if hit again
                healthBarActual.transform.parent.DOShakePosition(0.3f, strength: 10f, vibrato: 20);
            }
            
            // 2. Snap the actual health bar instantly
            healthBarActual.fillAmount = targetFill;

            // 3. Animate the ghost bar
            // Only start a new tween if the ghost is actually further ahead than the actual bar
            if (healthBarGhost.fillAmount > targetFill) 
            {
                // Complete: false ensures we don't snap to the end before restarting
                healthBarGhost.DOKill(false); 

                healthBarGhost.DOFillAmount(targetFill, 0.5f)
                    .SetDelay(0.5f)
                    .SetEase(Ease.OutQuad);
            }
            else 
            {
                // If healing, just snap the ghost bar to match
                healthBarGhost.fillAmount = targetFill;
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
