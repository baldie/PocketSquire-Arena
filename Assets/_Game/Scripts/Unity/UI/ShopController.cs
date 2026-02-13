using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Unity.Town;
using PocketSquire.Unity.UI;
using PocketSquire.Unity;



namespace PocketSquire.Arena.Unity.UI
{
    /// <summary>
    /// Controls the Shop UI window that displays purchasable items from a location.
    /// Reads shop item IDs from LocationData and cross-references with GameWorld.Items.
    /// </summary>
    public class ShopController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject shopWindow;
        [SerializeField] private Transform shopScrollContent;
        [SerializeField] private Button doneButton;

        [Tooltip("Reference to the game asset registry for loading sounds")]
        public GameAssetRegistry assetRegistry;

        [Header("Inventory Display")]
        [SerializeField] private TextMeshProUGUI inInventoryLabel;
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

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
            if (IsOpen && InputManager.GetButtonDown("Cancel"))
            {
                InputManager.ConsumeButton("Cancel");
                InputManager.ConsumeButton("Pause");
                Close();
            }
        }

        /// <summary>
        /// Opens the shop window and populates it with items from the location's shop inventory.
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

                    CreateItemRow(item);
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

        private void CreateItemRow(Item item)
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

            // Hook up the audio source for the item row
            var menuButtonSound = rowObj.GetComponent<MenuButtonSound>();
            if (menuButtonSound != null && audioSource != null)
            {
                menuButtonSound.source = audioSource;
            }

            // Ensure scale is correct
            rowObj.transform.localScale = Vector3.one;
            rowObj.transform.localPosition = Vector3.zero;

            // Configure MenuCursorTarget
            var cursorTarget = rowObj.GetComponent<MenuCursorTarget>();
            if (cursorTarget == null)
            {
                cursorTarget = rowObj.AddComponent<MenuCursorTarget>();
            }
            // Use default offset from MenuSelectionCursor on ShopWindow
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

            // Get sprite
            Sprite icon = null;
            if (!string.IsNullOrEmpty(item.Sprite))
            {
                icon = GameAssetRegistry.Instance.GetSprite(item.Sprite);
            }

            // Initialize row
            var row = rowObj.GetComponent<ItemRow>();
            if (row != null)
            {
                // Subscribe to selection to update inventory count labels
                row.OnSelected = (selectedItem) => UpdateInventoryDisplay(selectedItem);

                // Use the item's intrinsic stack size or just 1 for shop display, 
                // but the price field on ItemRow now handles the cost.
                row.Initialize(item, 1, icon, () => OnItemPurchased(item));

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

            // Set the label/count active (matching user design expectation)
            if (inInventoryLabel != null) inInventoryLabel.gameObject.SetActive(true);
            inventoryCountText.gameObject.SetActive(true);
        }

        private void UpdateGoldDisplay()
        {
            if (goldText == null) return;

            int gold = 0;
            if (GameState.Player != null)
            {
                gold = GameState.Player.Gold;
            }

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
                Debug.Log($"[ShopController] Cannot afford {item.Name} (Price: {item.Price}, Gold: {GameState.Player.Gold})");
                return;
            }

            // Play coins sound effect
            if (audioSource != null)
            {
                var coinsClip = assetRegistry?.GetSound("coin_spend");
                if (coinsClip != null)
                {
                    audioSource.PlayOneShot(coinsClip);
                } else {
                    Debug.LogWarning("[ShopController] No coins sound effect found");
                }
            } else {
                Debug.LogWarning("[ShopController] No audio source found");
            }

            // Update UI
            UpdateGoldDisplay();
            UpdateInventoryDisplay(item);

            Debug.Log($"[ShopController] Purchased {item.Name} for {item.Price} gold");
        }




        private void ClearShop()
        {
            foreach (var rowObj in spawnedRows)
            {
                if (rowObj != null)
                {
                    Destroy(rowObj);
                }
            }
            spawnedRows.Clear();
        }

        private void SelectFirstItem()
        {
            if (spawnedRows.Count > 0 && spawnedRows[0] != null)
            {
                EventSystem.current.SetSelectedGameObject(spawnedRows[0]);
            }
            else if (doneButton != null)
            {
                EventSystem.current.SetSelectedGameObject(doneButton.gameObject);
            }
        }
    }
}
