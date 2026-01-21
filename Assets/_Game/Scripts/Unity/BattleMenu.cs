using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PocketSquire.Arena.Core;

namespace PocketSquire.Unity
{
    public class BattleMenu : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject battleMenuUI;
        public Button firstSelectedButton;

        void Update()
        {

        }

        void Start()
        {
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
            GameWorld.Battle.CurrentTurn.End();
        }

        public void Defend()
        {
            Debug.Log("Defend");
            GameWorld.Battle.CurrentTurn.End();
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
