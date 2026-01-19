using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PocketSquire.Unity
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject pauseMenuUI;
        public Button firstSelectedButton;

        private bool isPaused = false;

        void Update()
        {
            // Check for the Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        void Start()
        {
            WireButtons();
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
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
                mainMenuBtn.onClick.AddListener(LoadMainMenu);

                var sound = mainMenuBtn.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }

            var quitBtn = pauseMenuUI.transform.Find("QuitButton")?.GetComponent<Button>();
            if (quitBtn != null)
            {
                quitBtn.onClick.RemoveAllListeners();
                quitBtn.onClick.AddListener(QuitGame);

                var sound = quitBtn.GetComponent<MenuButtonSound>();
                if (sound != null && audioSource != null) sound.source = audioSource;
            }
        }

        public void Resume()
        {
            pauseMenuUI.SetActive(false); // Hide UI
            Time.timeScale = 1f;          // Unfreeze time
            isPaused = false;
        }

        public void Pause()
        {
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
            Time.timeScale = 1f; // Ensure time is unfrozen
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
