using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace PocketSquire.Unity
{
    public class TownMenu : MonoBehaviour
    {

        private void Start()
        {
            WireButtons();

            // Default selection for gamepad/keyboard
            var adventureBtn = GameObject.Find("Canvas/Interactables/btn_adventure")?.GetComponent<Button>();
            if (adventureBtn != null)
            {
                adventureBtn.Select();
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
