using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PocketSquire.Unity
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadCharacterCreation()
        {
            SceneManager.LoadScene("CharacterCreation");
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
    }
}
