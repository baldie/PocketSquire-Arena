using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Unity.UI.LevelUp;

namespace PocketSquire.Unity
{
    public class CharacterCreationController : MonoBehaviour
    {
        [Header("Phase 1 - Gender Selection")]
        [SerializeField] private Image studentsImage;
        [SerializeField] private Button btnMale;
        [SerializeField] private Button btnFemale;

        [Header("Phase 2 - Attribute Allocation")]
        [SerializeField] private Image selectedSquireImage;
        [SerializeField] private GameObject levelUpBackground;
        [SerializeField] private LevelUpPresenter levelUpPresenter;

        [Header("Audio")]
        public AudioSource audioSource;

        // Internal state
        private Player.Genders _chosenGender;

        private void Awake()
        {
            // Phase 2 UI starts hidden
            if (levelUpBackground != null)
                levelUpBackground.SetActive(false);

            if (selectedSquireImage != null)
                selectedSquireImage.gameObject.SetActive(false);

            // Wire gender button clicks
            if (btnMale != null)
                btnMale.onClick.AddListener(() => OnGenderSelected(Player.Genders.m));

            if (btnFemale != null)
                btnFemale.onClick.AddListener(() => OnGenderSelected(Player.Genders.f));

            // Ensure we have a student image default state
            SetStudentsSprite("neither_selected");
        }

        // ─── Phase 1 Hover Callbacks (called by EventTrigger, wired via MCP) ───────

        public void OnMaleHover()
        {
            SetStudentsSprite("male_selected");
            PlaySound("male_huh");
        }

        public void OnFemaleHover()
        {
            SetStudentsSprite("female_selected");
            PlaySound("female_huh");
        }

        public void OnHoverExit()
        {
            SetStudentsSprite("neither_selected");
        }

        // ─── Phase 1 → Phase 2 ───────────────────────────────────────────────────

        private void OnGenderSelected(Player.Genders gender)
        {
            _chosenGender = gender;

            // Play chair push-back audio immediately
            PlaySound("chair_push_back");

            // Disable gender buttons
            if (btnMale != null) btnMale.interactable = false;
            if (btnFemale != null) btnFemale.interactable = false;

            // Show the selected squire idle image
            if (selectedSquireImage != null)
            {
                selectedSquireImage.gameObject.SetActive(true);
                string idleId = gender == Player.Genders.m ? "m_squire_idle" : "f_squire_idle";
                var idleSprite = GameAssetRegistry.Instance.GetSprite(idleId);
                if (idleSprite != null)
                    selectedSquireImage.sprite = idleSprite;
                else
                    Debug.LogWarning($"[CharacterCreation] Sprite '{idleId}' not found in registry.");
            }

            // Activate the LevelUp panel and initialise the presenter
            if (levelUpBackground != null)
                levelUpBackground.SetActive(true);

            InitialiseLevelUpPresenter();
        }

        private void InitialiseLevelUpPresenter()
        {
            if (levelUpPresenter == null)
            {
                Debug.LogError("[CharacterCreation] LevelUpPresenter reference is missing.");
                return;
            }

            // Resolve stat points for level 1 from the ProgressionSchedule
            int statPoints = 0;
            var schedule = GameAssetRegistry.Instance?.progressionSchedule;
            if (schedule != null)
            {
                var reward = schedule.GetRewardForLevel(1, PlayerClass.ClassName.Squire);
                statPoints = reward.StatPoints;
            }
            else
            {
                Debug.LogWarning("[CharacterCreation] ProgressionSchedule not found on GameAssetRegistry.");
            }

            // Build the default attribute dictionary (matches LevelUpPresenter's format)
            var defaultAttrs = Attributes.GetDefaultAttributes();
            var attrDict = new Dictionary<string, int>
            {
                { "STR", defaultAttrs.Strength },
                { "CON", defaultAttrs.Constitution },
                { "MAG", defaultAttrs.Magic },
                { "DEX", defaultAttrs.Dexterity },
                { "LCK", defaultAttrs.Luck },
                { "DEF", defaultAttrs.Defense }
            };

            // currentLevel = 0 because the player is brand new
            levelUpPresenter.SetOnAccept(OnAcceptClicked);
            levelUpPresenter.Initialize(attrDict, statPoints, currentLevel: 0);
        }

        // ─── Phase 3 – Accept & Transition ───────────────────────────────────────

        private void OnAcceptClicked()
        {
            // Create the player NOW so LevelUpPresenter.ApplyChangesToPlayer (called in its
            // 0.5s delayed _onAccept callback) has a non-null GameState.Player to write to.
            GameState.CreateNewGame(_chosenGender);

            // Play door-close and footsteps audio immediately
            PlaySound("class_door_close");
            PlaySound("footsteps");

            StartCoroutine(FadeAndTransition());
        }

        private IEnumerator FadeAndTransition()
        {
            // Add (or retrieve) a CanvasGroup on the root Canvas for the fade
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();

            CanvasGroup fadeGroup = null;
            if (canvas != null)
            {
                // Create a full-screen black overlay image as a child of the Canvas
                var overlayGO = new GameObject("FadeOverlay");
                overlayGO.transform.SetParent(canvas.transform, false);
                overlayGO.transform.SetAsLastSibling(); // render on top

                var rt = overlayGO.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var img = overlayGO.AddComponent<Image>();
                img.color = Color.black; // opaque — CanvasGroup.DOFade controls visibility

                fadeGroup = overlayGO.AddComponent<CanvasGroup>();
                fadeGroup.alpha = 0f;
                fadeGroup.blocksRaycasts = true;
            }

            // Fade to black over 1 second
            bool fadeDone = false;
            if (fadeGroup != null)
            {
                fadeGroup.DOFade(1f, 0.7f).SetUpdate(true).OnComplete(() => fadeDone = true);
            }
            else
            {
                fadeDone = true;
            }

            // Wait for fade to complete then save + transition
            yield return new WaitUntil(() => fadeDone);

            // Save after the visual transition — player already created in OnAcceptClicked
            SaveSystem.SaveGame(GameState.SelectedSaveSlot);

            // Start playtime tracking
            var tracker = FindFirstObjectByType<PlaytimeTracker>();
            if (tracker == null)
            {
                var trackerObj = new GameObject("PlaytimeTracker");
                tracker = trackerObj.AddComponent<PlaytimeTracker>();
            }
            tracker.StartTracking();

            // Transition to Town
            if (Application.CanStreamedLevelBeLoaded("Town"))
                SceneManager.LoadScene("Town");
            else
                Debug.LogWarning("[CharacterCreation] 'Town' scene is not in Build Settings.");
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private void SetStudentsSprite(string spriteId)
        {
            if (studentsImage == null) return;
            var sprite = GameAssetRegistry.Instance.GetSprite(spriteId);
            if (sprite != null)
                studentsImage.sprite = sprite;
            else
                Debug.LogWarning($"[CharacterCreation] Sprite '{spriteId}' not found in registry.");
        }

        private void PlaySound(string soundId)
        {
            if (audioSource == null) return;
            var clip = GameAssetRegistry.Instance.GetSound(soundId);
            if (clip != null)
                audioSource.PlayOneShot(clip);
            else
                Debug.LogWarning($"[CharacterCreation] Sound '{soundId}' not found in registry.");
        }
    }
}
