using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;
using PocketSquire.Arena.Core;
using TMPro;

namespace PocketSquire.Unity
{
    public class SaveSlotSelector : MonoBehaviour
    {
        private void Awake()
        {
            WireButtons();
        }

        private void Start()
        {
            LoadAllSlots();
        }

        private void LoadAllSlots()
        {
            UpdateSlotDisplay(SaveSlots.Slot1, "Canvas/Slot1Button");
            UpdateSlotDisplay(SaveSlots.Slot2, "Canvas/Slot2Button");
            UpdateSlotDisplay(SaveSlots.Slot3, "Canvas/Slot3Button");
        }

        private void UpdateSlotDisplay(SaveSlots slot, string buttonPath)
        {
            var data = SaveSystem.LoadGame(slot);
            if (data == null)
                return;

            var buttonObj = GameObject.Find(buttonPath);
            if (buttonObj == null)
                return;

            var textMesh = buttonObj.GetComponent<TextMeshProUGUI>();
            if (textMesh == null) textMesh = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (textMesh != null)
            {
                // PlayTime = #days, #hours, #minutes - MM/dd/YYYY ##:##:##
                var playTime = TimeSpan.FromTicks(data.PlayTimeTicks);
                var playTimeStr = string.Format("Play time {0}d {1}h {2}m", playTime.Days, playTime.Hours, playTime.Minutes);
                
                DateTime parsedDate = DateTime.MinValue;
                if (DateTime.TryParse(data.LastSaveDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
                    parsedDate = d;
                
                var dateStr = (parsedDate != DateTime.MinValue) ? parsedDate.ToString("MM/dd/yyyy HH:mm:ss") : "N/A";
                textMesh.text = $"{playTimeStr} - {dateStr}";
            }
        }

        private void WireButtons()
        {
            WireSlotButton(SaveSlots.Slot1, "Canvas/Slot1Button");
            WireSlotButton(SaveSlots.Slot2, "Canvas/Slot2Button");
            WireSlotButton(SaveSlots.Slot3, "Canvas/Slot3Button");
        }

        private void WireSlotButton(SaveSlots slot, string path)
        {
            var btn = GameObject.Find(path)?.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectSaveSlot(slot, btn.gameObject));
            }
        }

        public void SelectSaveSlot(SaveSlots slot, GameObject buttonObj)
        {
            var existingData = SaveSystem.LoadGame(slot);
            if (existingData != null)
            {
                GameState.LoadFromSaveData(existingData);
                Debug.Log($"[SaveSlotSelector] Loaded Save Slot: {slot}");
            }
            else
            {
                GameState.CreateNewGame(slot);
                // Immediately save to reserve the slot
                SaveSystem.SaveGame(slot, GameState.GetSaveData());
                Debug.Log($"[SaveSlotSelector] Created New Game in Slot: {slot}");
            }

            StartCoroutine(PlaySoundThenLoad("Town", buttonObj));
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
                Debug.LogWarning($"[SaveSlotSelector] Scene '{sceneName}' could not be loaded. Please ensure it is in Build Settings.");
            }
        }

        private void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
