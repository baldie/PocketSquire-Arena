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
        public enum SlotSelectionMode { NewGame, LoadGame }
        public static SlotSelectionMode Mode = SlotSelectionMode.NewGame;

        [Header("UI")]
        [SerializeField] private PocketSquire.Unity.UI.ConfirmationDialog confirmationDialog;

        private void Awake()
        {
            if (GameWorld.AllMonsters.Count == 0)
            {
                GameWorld.Load();
            }
            // Ensure we find the dialog if not assigned (fallback)
            if (confirmationDialog == null)
            {
                confirmationDialog = FindAnyObjectByType<PocketSquire.Unity.UI.ConfirmationDialog>(FindObjectsInactive.Include);
            }
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
                if (DateTime.TryParse(data.LastSaveDateString, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
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

            if (Mode == SlotSelectionMode.LoadGame)
            {
                // LOAD GAME MODE
                if (existingData != null)
                {
                    LoadGame(slot, existingData, buttonObj);
                }
                else
                {
                    // TODO: meaningful feedback for empty slot
                    Debug.Log("[SaveSlotSelector] Cannot load empty slot.");
                    // Maybe play a "buzzer" sound here if we had one
                }
            }
            else
            {
                // NEW GAME MODE
                if (existingData != null)
                {
                    // Slot occupied -> Confirm Overwrite
                    if (confirmationDialog != null)
                    {
                        PocketSquire.Unity.UI.ConfirmationDialog.Show(
                            confirmationDialog,
                            "Overwrite existing save?",
                            () => StartNewGame(slot, buttonObj) // On Confirm
                        );
                    }
                    else
                    {
                        Debug.LogError("[SaveSlotSelector] ConfirmationDialog reference missing! Overwriting anyway...");
                        StartNewGame(slot, buttonObj);
                    }
                }
                else
                {
                    // Slot empty -> Just start
                    StartNewGame(slot, buttonObj);
                }
            }
        }

        private void LoadGame(SaveSlots slot, SaveData data, GameObject buttonObj)
        {
            GameState.LoadFromSaveData(data);
            Debug.Log($"[SaveSlotSelector] Loaded Save Slot: {slot}");
            TransitionToTown(buttonObj);
        }

        private void StartNewGame(SaveSlots slot, GameObject buttonObj)
        {
            GameState.CreateNewGame(slot);
            // Immediately save to reserve the slot
            SaveSystem.SaveGame(slot);
            Debug.Log($"[SaveSlotSelector] Created New Game in Slot: {slot}");
            TransitionToTown(buttonObj);
        }

        private void TransitionToTown(GameObject buttonObj)
        {
            // Start playtime tracking for this save slot
            var tracker = FindFirstObjectByType<PlaytimeTracker>();
            if (tracker == null)
            {
                var trackerObj = new GameObject("PlaytimeTracker");
                tracker = trackerObj.AddComponent<PlaytimeTracker>();
            }
            tracker.StartTracking();

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
