using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Perks;

namespace PocketSquire.Arena.Unity.UI
{
    /// <summary>
    /// Manages the AcquiredPerkList panel — a scrollable overlay that lets the player
    /// swap a perk slot. Opened by a PerkUI slot button; closed after selection.
    /// </summary>
    public class AcquiredPerkListController : MonoBehaviour
    {
        [Header("Scroll View")]
        [Tooltip("The 'Content' RectTransform inside the ScrollRect")]
        [SerializeField] private Transform scrollContent;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private AudioSource audioSource;

        private List<GameObject> _rows = new List<GameObject>();
        private PerkUI _callerSlot;

        private void Awake()
        {
            gameObject.SetActive(false);

            if (scrollContent != null)
            {
                var rt = scrollContent.GetComponent<RectTransform>();
                if (rt != null) rt.pivot = new Vector2(0.5f, 1f);

                var vlg = scrollContent.GetComponent<VerticalLayoutGroup>();
                if (vlg != null) vlg.childAlignment = TextAnchor.UpperCenter;
            }
        }

        /// <summary>
        /// Populates and displays the list of acquired perks that can be slotted.
        /// </summary>
        public void Open(PerkUI callerSlot)
        {
            _callerSlot = callerSlot;

            var player = GameState.Player;
            if (player == null)
            {
                Debug.LogWarning("[AcquiredPerkListController] No player loaded — cannot open perk list.");
                return;
            }

            var prefab = GameAssetRegistry.Instance.itemRowPrefab;
            if (prefab == null || scrollContent == null)
            {
                Debug.LogError("[AcquiredPerkListController] Missing itemRowPrefab or scrollContent reference.");
                return;
            }

            Debug.Log($"[AcquiredPerkListController] Opening. Total AcquiredPerks: {player.AcquiredPerks.Count}. Active Perks: {player.ActiveArenaPerkIds.Count}");

            // --- "Remove" or "Empty slot" row first ---
            string topRowLabel = callerSlot.HasAssignedPerk ? "Remove" : "Empty slot";
            SpawnCustomRow(topRowLabel, null, prefab);

            // --- One row per acquired perk that is not currently active ---
            foreach (var perkId in player.AcquiredPerks)
            {
                if (player.ActiveArenaPerkIds.Contains(perkId))
                {
                    Debug.Log($"[AcquiredPerkListController] Skipping '{perkId}' because it is already active.");
                    continue;
                }

                var perk = GameWorld.GetArenaPerkById(perkId);
                if (perk == null)
                {
                    Debug.LogWarning($"[AcquiredPerkListController] Could not find perk definition for ID: '{perkId}'");
                    continue;
                }

                Debug.Log($"[AcquiredPerkListController] Spawning row for perk: '{perk.DisplayName}' ({perk.Id})");

                Sprite icon = null;
                if (!string.IsNullOrEmpty(perk.Icon))
                {
                    icon = GameAssetRegistry.Instance.GetSprite(perk.Icon);
                }

                SpawnPerkRow(perk, icon, prefab);
            }

            // Reset scroll position to top
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;

            gameObject.SetActive(true);
        }

        /// <summary>Closes the panel without making any changes.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
            _callerSlot = null;
        }

        private void SpawnCustomRow(string label, string perkId, GameObject prefab)
        {
            var go = Instantiate(prefab, scrollContent);
            SetupRowGameObject(go);
            _rows.Add(go);

            var row = go.GetComponent<ItemRow>();
            if (row == null) return;

            var capturedId = perkId; // null → "Remove"
            row.InitializeCustom(label, () =>
            {
                _callerSlot?.AssignPerk(capturedId);
                Close();
            });
        }

        private void SpawnPerkRow(ArenaPerk perk, Sprite icon, GameObject prefab)
        {
            var go = Instantiate(prefab, scrollContent);
            SetupRowGameObject(go);
            _rows.Add(go);

            var row = go.GetComponent<ItemRow>();
            if (row == null) return;

            var capturedPerk = perk;
            row.Initialize(perk, icon, () =>
            {
                _callerSlot?.AssignPerk(capturedPerk.Id);
                Close();
            }, showPrice: false);
            row.HidePriceText();
        }

        /// <summary>
        /// Triggers right before the window closes naturally (e.g. from an outside click).
        /// Re-clears the spawned children.
        /// </summary>
        private void OnDisable()
        {
            foreach (var go in _rows)
            {
                if (go != null) Destroy(go);
            }
            _rows.Clear();
        }

        private void SetupRowGameObject(GameObject go)
        {
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;

            var layoutElement = go.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = go.AddComponent<LayoutElement>();
                layoutElement.minHeight = 100f;
                layoutElement.preferredHeight = 100f;
                layoutElement.flexibleWidth = 1f;
            }

            if (audioSource != null)
            {
                var menuButtonSound = go.GetComponent<MenuButtonSound>();
                if (menuButtonSound != null)
                {
                    menuButtonSound.source = audioSource;
                }
            }
        }
    }
}
