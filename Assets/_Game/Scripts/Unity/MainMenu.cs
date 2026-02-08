using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;
using PocketSquire.Arena.Core;

namespace PocketSquire.Unity
{
    public class MainMenu : MonoBehaviour
    {
        private void Awake()
        {
            if (GameWorld.AllMonsters.Count == 0)
            {
                GameWorld.Load();
            }
            WireButtons();
        }


        private void Start()
        {
            var continueBtn = GameObject.Find("Canvas/ContinueButton")?.GetComponent<Button>();
            if (continueBtn != null && continueBtn.interactable)
            {
                continueBtn.Select();
            }
            else
            {
                GameObject.Find("Canvas/NewGameButton")?.GetComponent<Button>()?.Select();
            }
        }

        private void WireButtons()
        {
            var continueButton = GameObject.Find("Canvas/ContinueButton")?.GetComponent<Button>();
            if (continueButton != null)
            {
                SaveData?[] saves = new SaveData?[]
                {
                    SaveSystem.LoadGame(SaveSlots.Slot1),
                    SaveSystem.LoadGame(SaveSlots.Slot2),
                    SaveSystem.LoadGame(SaveSlots.Slot3)
                };

                var mostRecent = GameState.FindMostRecentSave(saves);
                if (mostRecent == null)
                {
                    continueButton.interactable = false;
                    var nav = continueButton.navigation;
                    nav.mode = Navigation.Mode.None;
                    continueButton.navigation = nav;
                }
                else
                {
                    continueButton.interactable = true;
                    var nav = continueButton.navigation;
                    nav.mode = Navigation.Mode.Automatic;
                    continueButton.navigation = nav;

                    continueButton.onClick.RemoveAllListeners();
                    continueButton.onClick.AddListener(() => {
                        GameState.LoadFromSaveData(mostRecent);
                        GoToScene("Town", continueButton.gameObject);
                    });
                }
            }

            var newGameButton = GameObject.Find("Canvas/NewGameButton")?.GetComponent<Button>();
            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveAllListeners();
                newGameButton.onClick.AddListener(() => {
                    SaveSlotSelector.Mode = SaveSlotSelector.SlotSelectionMode.NewGame;
                    GoToScene("SaveSlotSelection", newGameButton.gameObject);
                });
            }

            var loadGameButton = GameObject.Find("Canvas/LoadGameButton")?.GetComponent<Button>();
            if (loadGameButton != null)
            {
                loadGameButton.interactable = true; 
                // We could check if there are ANY saves, but SaveSystem helper to check "Any" isn't strictly here.
                // For now, let them go to the scene, and empty slots will just be unselectable.

                loadGameButton.onClick.RemoveAllListeners();
                loadGameButton.onClick.AddListener(() => {
                    SaveSlotSelector.Mode = SaveSlotSelector.SlotSelectionMode.LoadGame;
                    GoToScene("SaveSlotSelection", loadGameButton.gameObject);
                });
            }

            var optionsButton = GameObject.Find("Canvas/OptionsButton")?.GetComponent<Button>();
            if (optionsButton != null)
            {
                optionsButton.onClick.RemoveAllListeners();
                optionsButton.onClick.AddListener(() => Debug.Log("Options Clicked"));
            }

            var exitButton = GameObject.Find("Canvas/ExitButton")?.GetComponent<Button>();
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(ExitGame);
            }
        }

        public void GoToScene(string sceneName, GameObject buttonObj)
        {
            StartCoroutine(PlaySoundThenLoad(sceneName, buttonObj));
        }

        IEnumerator PlaySoundThenLoad(string sceneName, GameObject buttonObj)
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
                Debug.LogWarning($"[MainMenu] Scene '{sceneName}' could not be loaded. Please ensure it is in Build Settings.");
            }
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
