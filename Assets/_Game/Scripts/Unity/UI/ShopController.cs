using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Unity.Town;
using PocketSquire.Unity.UI;

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

        [Header("Assets")]
        [SerializeField] private GameObject itemRowPrefab;
        [SerializeField] private GameAssetRegistry gameAssetRegistry;

        [Header("Inventory Display")]
        [SerializeField] private TextMeshProUGUI inInventoryLabel;
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private TextMeshProUGUI goldText;

        private readonly List<GameObject> spawnedRows = new List<GameObject>();
        private LocationData currentLocation;

        /// <summary>
        /// Callback invoked when the shop window is closed.
        /// TownUIManager uses this to restore dialogue options.
        /// </summary>
        public System.Action OnShopClosed { get; set; }

        private void Awake()
        {
            if (doneButton != null)
            {
                doneButton.onClick.RemoveAllListeners();
                doneButton.onClick.AddListener(Close);
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
            if (shopScrollContent == null || itemRowPrefab == null)
            {
                Debug.LogWarning("[ShopController] Missing shopScrollContent or itemRowPrefab");
                return;
            }

            var rowObj = Instantiate(itemRowPrefab, shopScrollContent);
            rowObj.SetActive(true);
            spawnedRows.Add(rowObj);

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
            if (gameAssetRegistry != null && !string.IsNullOrEmpty(item.Sprite))
            {
                icon = gameAssetRegistry.GetSprite(item.Sprite);
            }

            // Initialize row
            var row = rowObj.GetComponent<ItemRow>();
            if (row != null)
            {
                // Subscribe to selection to update inventory count labels
                row.OnSelected = (selectedItem) => UpdateInventoryDisplay(selectedItem);

                // Use the item's intrinsic stack size or just 1 for shop display, 
                // but the price field on ItemRow now handles the cost.
                row.Initialize(item, 1, icon, () =>
                {
                    // TODO: Future - handle purchase logic
                    Debug.Log($"[ShopController] Clicked item: {item.Name} (Price: {item.Price})");
                });
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
