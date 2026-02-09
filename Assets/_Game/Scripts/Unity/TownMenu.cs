using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Unity.Town;

namespace PocketSquire.Unity
{
    public class TownMenu : MonoBehaviour
    {
        [Header("Location Data")]
        [SerializeField] private LocationData blacksmithLocation;
        [SerializeField] private LocationData merchantLocation;
        [SerializeField] private LocationData towerLocation;
        [SerializeField] private LocationData archeryLocation;
        [SerializeField] private LocationData homeLocation;

        private TownUIManager townUIManager;

        private void Start()
        {
            // Ensure GameWorld is loaded - this allows us to start immediately in the town
            if (GameWorld.AllMonsters.Count == 0 || GameState.Player == null)
            {
                if (GameWorld.AllMonsters.Count == 0) GameWorld.Load();
                if (GameState.Player == null) GameState.CreateNewGame(SaveSlots.Unknown);
            }

            // Find the TownUIManager
            townUIManager = FindFirstObjectByType<TownUIManager>(FindObjectsInactive.Include);

            WireButtons();

            // Default selection for gamepad/keyboard
            var adventureBtn = GameObject.Find("Canvas/TownMapPanel/Interactables/btn_adventure")?.GetComponent<Button>();
            if (adventureBtn != null)
            {
                adventureBtn.Select();
            }

            if (GameState.CurrentRun != null){
                // Must have lost in the arena :(
                GameState.CurrentRun.Reset();
            }

            // Save the game (skip if testing with Unknown slot)
            if (GameState.SelectedSaveSlot != SaveSlots.Unknown)
            {
                SaveSystem.SaveGame(GameState.SelectedSaveSlot, GameState.GetSaveData());
            }
            else
            {
                Debug.Log("[TownMenu] Skipping save for testing mode (Unknown slot)");
            }
        }


        private void WireButtons()
        {
            var uiAudio = GameObject.Find("UIAudio");
            var audioSource = uiAudio != null ? uiAudio.GetComponent<AudioSource>() : null;

            // Adventure button - goes to Arena scene
            var adventureBtn = GameObject.Find("Canvas/TownMapPanel/Interactables/btn_adventure")?.GetComponent<Button>();
            if (adventureBtn != null)
            {
                adventureBtn.onClick.RemoveAllListeners();
                adventureBtn.onClick.AddListener(() => StartCoroutine(PlaySoundAndLoadCoroutine("Arena", adventureBtn.gameObject)));
                
                var sound = adventureBtn.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }
            
            // Wire up town locations to TownUIManager
            WireLocationButton("btn_blacksmith", blacksmithLocation, audioSource);
            WireLocationButton("btn_merchant", merchantLocation, audioSource);
            WireLocationButton("btn_tower", towerLocation, audioSource);
            WireLocationButton("btn_archery", archeryLocation, audioSource);
            WireLocationButton("btn_home", homeLocation, audioSource);
        }

        private void WireLocationButton(string buttonPath, LocationData locationData, AudioSource audioSource)
        {
            if (townUIManager == null || locationData == null) return;

            var button = GameObject.Find("Canvas/TownMapPanel/Interactables/" + buttonPath)?.GetComponent<Button>();
            if (button == null) return;
            
            button.onClick.RemoveAllListeners();
            // Pass the button's GameObject for the punch animation
            button.onClick.AddListener(() => townUIManager.ShowInteriorWithTransition(locationData, button.gameObject));

            var sound = button.GetComponent<MenuButtonSound>();
            if (sound != null && audioSource != null) sound.source = audioSource;
        }

        private IEnumerator PlaySoundAndLoadCoroutine(string sceneName, GameObject buttonObj)
        {
            if (buttonObj != null)
            {
                var menuButtonSound = buttonObj.GetComponent<MenuButtonSound>();
                if (menuButtonSound != null && menuButtonSound.clickSound != null)
                {
                    menuButtonSound.PlayClick();
                    yield return new WaitForSecondsRealtime(menuButtonSound.clickSound.length);
                }
            }

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"[TownMenu] Scene '{sceneName}' could not be loaded. Please ensure it is in Build Settings.");
            }
        }
    }
}
