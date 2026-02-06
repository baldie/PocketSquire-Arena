using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using PocketSquire.Arena.Core;

namespace PocketSquire.Unity
{
    public class TownMenu : MonoBehaviour
    {

        private void Start()
        {
            // Ensure GameWorld is loaded - this allows us to start immediately in the town
            if (GameWorld.AllMonsters.Count == 0 || GameState.Player == null)
            {
                if (GameWorld.AllMonsters.Count == 0) GameWorld.Load();
                if (GameState.Player == null) GameState.CreateNewGame(SaveSlots.Unknown);
            }

            WireButtons();

            // Default selection for gamepad/keyboard
            var adventureBtn = GameObject.Find("Canvas/Interactables/btn_adventure")?.GetComponent<Button>();
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

            var adventureBtn = GameObject.Find("Canvas/Interactables/btn_adventure")?.GetComponent<Button>();
            if (adventureBtn != null)
            {
                adventureBtn.onClick.RemoveAllListeners();
                adventureBtn.onClick.AddListener(() => StartCoroutine(PlaySoundThenLoad("Arena", adventureBtn.gameObject)));
                
                var sound = adventureBtn.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }
            
            //TODO: Wire up town locations here
        }

        private IEnumerator PlaySoundThenLoad(string sceneName, GameObject buttonObj)
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
