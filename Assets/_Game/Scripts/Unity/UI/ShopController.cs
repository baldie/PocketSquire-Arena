using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.LevelUp;
using PocketSquire.Arena.Unity.LevelUp;
using PocketSquire.Arena.Unity.Town;
using PocketSquire.Unity.UI;
using PocketSquire.Unity;



namespace PocketSquire.Arena.Unity.UI
{
    /// <summary>
    /// Controls the Shop UI window that displays purchasable items and perks from a location.
    /// Reads shop item IDs and perk nodes from LocationData.
    /// Perks that the player already owns are hidden from the shop listing.
    /// </summary>
    public class ShopController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject shopWindow;
        [SerializeField] private Transform shopScrollContent;
        [SerializeField] private Button doneButton;

        [Header("Inventory Display")]
        [SerializeField] private TextMeshProUGUI inInventoryLabel;
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        [Header("Toast")]
        [SerializeField] private InteriorToast interiorToast;

        public bool IsOpen => shopWindow != null && shopWindow.activeInHierarchy;

        private readonly List<GameObject> spawnedRows = new List<GameObject>();
        private LocationData currentLocation;

        /// <summary>
        /// Callback invoked when the shop window is closed.
        /// TownUIManager uses this to restore dialogue options.
        /// </summary>
        public System.Action OnShopClosed { get; set; }

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>() ?? GetComponentInParent<AudioSource>();
                if (audioSource == null)
                {
                    var uiAudio = GameObject.Find("UIAudio");
                    if (uiAudio != null) audioSource = uiAudio.GetComponent<AudioSource>();
                }
            }

            if (doneButton != null)
            {
                doneButton.onClick.RemoveAllListeners();
                doneButton.onClick.AddListener(Close);
            }
        }

        private void Update()
        {
            if (IsOpen && GameInput.Instance.GetButtonDown(GameInput.Instance.CancelAction))
            {
                GameInput.Instance.ConsumeButton(GameInput.Instance.CancelAction);
                GameInput.Instance.ConsumeButton(GameInput.Instance.PauseAction);
                Close();
            }
        }

        /// <summary>
        /// Opens the shop window and populates it with items and perks from the location.
        /// Already-owned perks are excluded from the listing.
        /// </summary>
        public void Open(LocationData location)
        {
            if (location == null)
            {
                Debug.LogWarning("[ShopController] Open called with null LocationData");
                return;
            }

            currentLocation = location;
            ClearShop();

            // Populate shop items
            if (location.ShopItemIds != null && location.ShopItemIds.Count > 0)
            {
                foreach (int itemId in location.ShopItemIds)
                {
                    var item = GameWorld.GetItemById(itemId);
                    if (item == null)
                    {
                        Debug.LogWarning($"[ShopController] Item ID {itemId} not found in GameWorld.Items");
                        continue;
                    }

                    Sprite icon = null;
                    if (!string.IsNullOrEmpty(item.Sprite))
                        icon = GameAssetRegistry.Instance.GetSprite(item.Sprite);

                    CreateMerchandiseRow(item, icon, () => OnItemPurchased(item));
                }
            }

            // Populate shop perks — skip any the player already owns
            if (location.ShopPerkNodes != null && location.ShopPerkNodes.Count > 0)
            {
                var ownedPerks = GameState.Player?.UnlockedPerks ?? new System.Collections.Generic.HashSet<string>();

                foreach (var perkNode in location.ShopPerkNodes)
                {
                    if (perkNode == null) continue;

                    // Hide perks the player already has
                    if (ownedPerks.Contains(perkNode.Id)) continue;

                    var corePerk = perkNode.ToCorePerk();
                    CreateMerchandiseRow(corePerk, perkNode.Icon, () => OnPerkPurchased(perkNode), $"PerkRow_{perkNode.Id}");
                }
            }

            // Force layout rebuild
            if (shopScrollContent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(shopScrollContent as RectTransform);
            }

            // Activate window
            if (shopWindow != null)
            {
                shopWindow.SetActive(true);
            }

            // Update gold display
            UpdateGoldDisplay();

            // Select first item
            SelectFirstItem();
        }

        /// <summary>
        /// Closes the shop window and fires the OnShopClosed callback.
        /// </summary>
        public void Close()
        {
            ClearShop();

            if (shopWindow != null)
            {
                shopWindow.SetActive(false);
            }

            OnShopClosed?.Invoke();
        }

        /// <summary>
        /// Creates a single merchandise row for any IMerchandise (item or perk).
        /// </summary>
        private void CreateMerchandiseRow(IMerchandise merchandise, Sprite icon, System.Action onPurchase, string rowName = null)
        {
            var prefab = GameAssetRegistry.Instance.itemRowPrefab;
            if (shopScrollContent == null || prefab == null)
            {
                Debug.LogWarning("[ShopController] Missing shopScrollContent or itemRowPrefab in GameAssetRegistry");
                return;
            }

            var rowObj = Instantiate(prefab, shopScrollContent);
            rowObj.SetActive(true);
            spawnedRows.Add(rowObj);

            // Hook up audio
            var menuButtonSound = rowObj.GetComponent<MenuButtonSound>();
            if (menuButtonSound != null && audioSource != null)
                menuButtonSound.source = audioSource;

            rowObj.transform.localScale = Vector3.one;
            rowObj.transform.localPosition = Vector3.zero;
            if (!string.IsNullOrEmpty(rowName)) rowObj.name = rowName;

            // Configure MenuCursorTarget
            var cursorTarget = rowObj.GetComponent<MenuCursorTarget>();
            if (cursorTarget == null) cursorTarget = rowObj.AddComponent<MenuCursorTarget>();
            cursorTarget.cursorOffset = new Vector3(-60f, 0f, 0f);
            cursorTarget.useLocalOffset = true;

            // Ensure LayoutElement exists
            var layoutElement = rowObj.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = rowObj.AddComponent<LayoutElement>();
                layoutElement.minHeight = 100f;
                layoutElement.preferredHeight = 100f;
                layoutElement.flexibleWidth = 1f;
            }

            // Try MerchandiseRow first, then fall back to legacy ItemRow
            var merchandiseRow = rowObj.GetComponent<MerchandiseRow>();
            if (merchandiseRow != null)
            {
                merchandiseRow.OnSelected = (m) => UpdateInventoryDisplayForMerchandise(m);
                merchandiseRow.Initialize(merchandise, icon, onPurchase);
            }
            else
            {
                // Legacy fallback: prefab still has ItemRow — works for both items and perks
                // via the new IMerchandise overload.
                var itemRow = rowObj.GetComponent<ItemRow>();
                if (itemRow != null)
                {
                    if (merchandise is Item item)
                    {
                        itemRow.OnSelected = (selectedItem) => UpdateInventoryDisplay(selectedItem);
                        itemRow.Initialize(item, 1, icon, onPurchase);
                    }
                    else
                    {
                        itemRow.OnMerchandiseSelected = (m) => UpdateInventoryDisplayForMerchandise(m);
                        itemRow.Initialize(merchandise, icon, onPurchase);
                    }
                }
            }
        }

        private void UpdateInventoryDisplayForMerchandise(IMerchandise merchandise)
        {
            // Only items have inventory counts; hide the label for perks
            if (merchandise is Item item)
            {
                UpdateInventoryDisplay(item);
            }
            else
            {
                // Perk selected — hide inventory count
                if (inInventoryLabel != null) inInventoryLabel.gameObject.SetActive(false);
                if (inventoryCountText != null) inventoryCountText.gameObject.SetActive(false);
            }
        }

        private void UpdateInventoryDisplay(Item item)
        {
            if (item == null || inventoryCountText == null) return;

            int count = 0;
            if (GameState.Player != null && GameState.Player.Inventory != null)
            {
                count = GameState.Player.Inventory.GetItemCount(item.Id);
            }

            inventoryCountText.text = count.ToString();
            if (inInventoryLabel != null) inInventoryLabel.gameObject.SetActive(true);
            inventoryCountText.gameObject.SetActive(true);
        }

        private void UpdateGoldDisplay()
        {
            if (goldText == null) return;
            int gold = GameState.Player?.Gold ?? 0;
            goldText.text = gold.ToString();
        }

        private void OnItemPurchased(Item item)
        {
            if (GameState.Player == null)
            {
                Debug.LogWarning("[ShopController] Cannot purchase - no player");
                return;
            }

            if (!GameState.Player.TryPurchaseItem(item))
            {
                PlayDeniedSound();
                if (item.Price >= GameState.Player.Gold)
                    interiorToast.ShowToast("Not enough gold");
                else
                    interiorToast.ShowToast("Can't carry any more items");
                return;
            }

            PlayCoinSound();
            UpdateGoldDisplay();
            UpdateInventoryDisplay(item);
            Debug.Log($"[ShopController] Purchased item '{item.Name}' for {item.Price} gold");
        }

        private void OnPerkPurchased(PerkNode perkNode)
        {
            if (GameState.Player == null)
            {
                Debug.LogWarning("[ShopController] Cannot purchase perk - no player");
                return;
            }

            var perk = perkNode.ToCorePerk();

            if (!GameState.Player.TryPurchasePerk(perk))
            {
                PlayDeniedSound();
                // TryPurchasePerk returns false for already-owned too, but those rows are never shown
                interiorToast.ShowToast("Not enough gold");
                return;
            }

            PlayCoinSound();
            UpdateGoldDisplay();
            Debug.Log($"[ShopController] Purchased perk '{perk.DisplayName}' for {perk.Price} gold");

            // Remove the purchased perk row so it cannot be bought again this session
            RemoveRowForPerkId(perkNode.Id);
        }

        /// <summary>
        /// Removes and destroys the row associated with the given perk ID after purchase.
        /// </summary>
        private void RemoveRowForPerkId(string perkId)
        {
            string targetName = $"PerkRow_{perkId}";
            for (int i = spawnedRows.Count - 1; i >= 0; i--)
            {
                var rowObj = spawnedRows[i];
                if (rowObj != null && rowObj.name == targetName)
                {
                    Destroy(rowObj);
                    spawnedRows.RemoveAt(i);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(shopScrollContent as RectTransform);
                    return;
                }
            }
            Debug.LogWarning($"[ShopController] Could not find row to remove for perk '{perkId}'");
        }

        private void PlayCoinSound()
        {
            if (audioSource == null)
            {
                Debug.LogWarning("[ShopController] No audio source found");
                return;
            }
            var clip = GameAssetRegistry.Instance.GetSound("coin_spend");
            if (clip != null) audioSource.PlayOneShot(clip);
            else Debug.LogWarning("[ShopController] No coins sound effect found");
        }

        private void PlayDeniedSound()
        {
            var clip = GameAssetRegistry.Instance.GetSound("denied");
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }

        private void ClearShop()
        {
            foreach (var rowObj in spawnedRows)
            {
                if (rowObj != null) Destroy(rowObj);
            }
            spawnedRows.Clear();
        }

        private void SelectFirstItem()
        {
            if (spawnedRows.Count > 0 && spawnedRows[0] != null)
                EventSystem.current.SetSelectedGameObject(spawnedRows[0]);
            else if (doneButton != null)
                EventSystem.current.SetSelectedGameObject(doneButton.gameObject);
        }
    }
}
