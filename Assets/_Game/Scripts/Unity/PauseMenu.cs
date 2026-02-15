using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PocketSquire.Unity.UI;
using PocketSquire.Arena.Unity.UI;

namespace PocketSquire.Unity
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject pauseMenuUI;
        public Button firstSelectedButton;
        public ConfirmationDialog confirmationDialog;
        private GameObject selectedObj;

        private bool isPaused = false;

        void Update()
        {
            // 1. Toggle Pause (Escape Key OR Gamepad Start)
            if (InputManager.GetButtonDown("Pause")) 
            {
                InputManager.ConsumeButton("Pause");
                InputManager.ConsumeButton("Cancel");
                if (isPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
            // 2. Allow "B" button (Cancel) to close the menu only, 
            // but only if we didn't just handle the Pause input
            else if (isPaused && InputManager.GetButtonDown("Cancel"))
            {
                InputManager.ConsumeButton("Pause");
                InputManager.ConsumeButton("Cancel");
                Resume();
            }
        }

        void Start()
        {
            if (confirmationDialog == null || !confirmationDialog.gameObject.scene.IsValid())
            {
                confirmationDialog = FindAnyObjectByType<ConfirmationDialog>(FindObjectsInactive.Include);
            }

            WireButtons();
            if (pauseMenuUI != null) {
                pauseMenuUI.SetActive(false);
            }
        }

        private void WireButtons()
        {
            if (pauseMenuUI == null) return;

            var uiAudio = GameObject.Find("UIAudio");
            var audioSource = uiAudio != null ? uiAudio.GetComponent<AudioSource>() : null;

            var optionsBtn = pauseMenuUI.transform.Find("OptionsButton")?.GetComponent<Button>();
            if (optionsBtn != null)
            {
                optionsBtn.onClick.RemoveAllListeners();
                optionsBtn.onClick.AddListener(() => Debug.Log("Options Clicked"));
                if (firstSelectedButton == null) firstSelectedButton = optionsBtn;

                var sound = optionsBtn.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            var mainMenuBtn = pauseMenuUI.transform.Find("MainMenuButton")?.GetComponent<Button>();
            if (mainMenuBtn != null)
            {
                mainMenuBtn.onClick.RemoveAllListeners();
                mainMenuBtn.onClick.AddListener(() => 
                {
                    pauseMenuUI.SetActive(false); // Hide pause menu when showing confirmation
                    ConfirmationDialog.Show(
                        confirmationDialog,
                        "Are you sure?",
                        LoadMainMenu,
                        onCancel: () => {
                            // select a button right away or the cursor will be in the wrong place
                            var button = firstSelectedButton ?? optionsBtn;
                            EventSystem.current.SetSelectedGameObject(button.gameObject);
                            pauseMenuUI.SetActive(true); // Restore pause menu on cancel
                        }
                    );
                });

                var sound = mainMenuBtn.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            var quitBtn = pauseMenuUI.transform.Find("QuitButton")?.GetComponent<Button>();
            if (quitBtn != null)
            {
                quitBtn.onClick.RemoveAllListeners();
                quitBtn.onClick.AddListener(() => 
                {
                    pauseMenuUI.SetActive(false); // Hide pause menu when showing confirmation
                    bool isTown = SceneManager.GetActiveScene().name == "Town";
                    string quitMessage = isTown 
                        ? "Save and quit?" 
                        : "Are you sure? All your progress since last save will be lost";

                    ConfirmationDialog.Show(
                        confirmationDialog,
                        quitMessage,
                        QuitGame,
                        onCancel: () => {
                            // select a button right away or the cursor will be in the wrong place
                            var button = firstSelectedButton ?? optionsBtn;
                            EventSystem.current.SetSelectedGameObject(button.gameObject);
                            pauseMenuUI.SetActive(true); // Restore pause menu on cancel
                        }
                    );
                });

                var sound = quitBtn.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }
        }

        public void Resume()
        {
            pauseMenuUI.SetActive(false); // Hide UI
            if (selectedObj != null)
            {
                EventSystem.current.SetSelectedGameObject(selectedObj);
            }
            Time.timeScale = 1f;          // Unfreeze time
            isPaused = false;
        }

        public void Pause()
        {
            Debug.Log("Pause() called");
            // Close PlayerMenu if it's open
            var playerMenu = FindFirstObjectByType<PlayerMenuController>();
            if (playerMenu != null && playerMenu.IsOpen)
            {
                InputManager.ConsumeButton("Pause");
                InputManager.ConsumeButton("Cancel");
                playerMenu.Close();
                return;
            }

            // Close ShopMenu if it's open
            var shopMenu = FindFirstObjectByType<ShopController>();
            if (shopMenu != null && shopMenu.IsOpen)
            {
                InputManager.ConsumeButton("Pause");
                InputManager.ConsumeButton("Cancel");
                shopMenu.Close();
                return;
            }

            selectedObj = EventSystem.current.currentSelectedGameObject;
            pauseMenuUI.SetActive(true);  // Show UI
            Time.timeScale = 0f;          // Freeze time
            isPaused = true;

            if (firstSelectedButton != null)
            {
                firstSelectedButton.Select();
            }
        }

        public void LoadMainMenu()
        {
            // Stop playtime tracking and save before returning to main menu
            var tracker = FindFirstObjectByType<PlaytimeTracker>();
            if (tracker != null)
            {
                tracker.StopTracking();
            }
            
            // Save the game (automatically accumulates playtime via SaveSystem)
            SaveSystem.SaveGame(PocketSquire.Arena.Core.GameState.SelectedSaveSlot);
            
            // Reset save slot to prevent tracking in main menu
            PocketSquire.Arena.Core.GameState.SelectedSaveSlot = PocketSquire.Arena.Core.SaveSlots.Unknown;
            
            Time.timeScale = 1f; // Ensure time is unfrozen
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
            // Save if we are in the Town scene before exiting
            if (SceneManager.GetActiveScene().name == "Town")
            {
                var tracker = FindFirstObjectByType<PlaytimeTracker>();
                if (tracker != null)
                {
                    tracker.StopTracking();
                }
                SaveSystem.SaveGame(PocketSquire.Arena.Core.GameState.SelectedSaveSlot);
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
