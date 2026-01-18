using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace PocketSquire.Unity
{
    public class SceneLoader : MonoBehaviour
    {
        private void Awake()
        {
            WireButtons();
        }

        private void WireButtons()
        {
            var newGameButton = GameObject.Find("Canvas/NewGameButton")?.GetComponent<Button>();
            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveAllListeners();
                newGameButton.onClick.AddListener(ChooseSaveSlot);
                Debug.Log("Wired up NewGameButton");
            }

            var optionsButton = GameObject.Find("Canvas/OptionsButton")?.GetComponent<Button>();
            if (optionsButton != null)
            {
                optionsButton.onClick.RemoveAllListeners();
                optionsButton.onClick.AddListener(() => Debug.Log("Options Clicked"));
                Debug.Log("Wired up OptionsButton");
            }

            var exitButton = GameObject.Find("Canvas/ExitButton")?.GetComponent<Button>();
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(ExitGame);
                Debug.Log("Wired up ExitButton");
            }
        }

        public void ChooseSaveSlot()
        {
            StartCoroutine(PlaySoundThenLoad());
        }

        IEnumerator PlaySoundThenLoad()
        {
            // 1. Play the sound
            var newGameButton = GameObject.Find("Canvas/NewGameButton");
            var menuButtonSound = newGameButton.GetComponent<MenuButtonSound>();
            menuButtonSound.PlayClick();

            // 2. Wait for the clip to finish (or a set amount of time)
            // Use real-time to ensure it works even if timeScale is 0
            yield return new WaitForSecondsRealtime(menuButtonSound.clickSound.length);

            // 3. Load the scene
            SceneManager.LoadScene("SaveSlotSelection");
        }

        private void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                if (SceneManager.GetActiveScene().name == "SaveSlotSelection")
                {
                    LoadMainMenu();
                }
            }
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
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
