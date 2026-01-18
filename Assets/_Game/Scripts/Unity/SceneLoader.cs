using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace PocketSquire.Unity
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void Awake()
        {
            WireButtons();
        }

        private void WireButtons()
        {
            var playButton = GameObject.Find("Canvas/PlayButton")?.GetComponent<Button>();
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(LoadCharacterCreation);
                Debug.Log("Wired up PlayButton");
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

        public void LoadCharacterCreation()
        {
            StartCoroutine(PlaySoundThenLoad());
        }

        IEnumerator PlaySoundThenLoad()
        {
            // 1. Play the sound
            var playButton = GameObject.Find("Canvas/PlayButton");
            var menuButtonSound = playButton.GetComponent<MenuButtonSound>();
            menuButtonSound.PlayClick();

            // 2. Wait for the clip to finish (or a set amount of time)
            // Use real-time to ensure it works even if timeScale is 0
            yield return new WaitForSecondsRealtime(menuButtonSound.clickSound.length);

            // 3. Load the scene
            SceneManager.LoadScene("CharacterCreation");
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
