using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using PocketSquire.Arena.Core;
using UnityEngine.EventSystems;
using PocketSquire.Unity.UI;
using PocketSquire.Unity;

namespace PocketSquire.Arena.Unity.UI
{
    public class ItemSelectionDialog : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform dialogPanel;
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject itemRowPrefab; // Should have ItemRow component
        [SerializeField] private TextMeshProUGUI emptyMessage;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip cancelSound; // Optional, might use ConfirmationDialog's sounds if shared

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;

        private Action<int> _onItemSelected; 
        private Action _onCancel;
        private CanvasGroup _canvasGroup;
        private List<GameObject> _instantiatedRows = new List<GameObject>();
        private GameAssetRegistry _registry;

        private void Awake()
        {
             InitializeCanvasGroup();

             // Auto-wire references if missing
             if (dialogPanel == null) dialogPanel = GetComponent<RectTransform>();
             if (contentContainer == null) contentContainer = transform.Find("ContentRoot");
             
             if (itemRowPrefab == null)
             {
                 var template = transform.root.Find("ItemRow_Template"); // Used root.Find just in case, or transform.parent
                 if (template == null && transform.parent != null) template = transform.parent.Find("ItemRow_Template");
                 
                 if (template != null)
                 {
                     itemRowPrefab = template.gameObject;
                     itemRowPrefab.SetActive(false);
                 }
             }

             // Auto-wire MenuSelectionCursor
             var cursor = GetComponent<MenuSelectionCursor>();
             if (cursor == null) cursor = gameObject.AddComponent<MenuSelectionCursor>();
             
             if (cursor.cursorGraph == null)
             {
                 var cursorObj = transform.Find("SelectionCursor");
                 if (cursorObj != null)
                 {
                     cursor.cursorGraph = cursorObj.GetComponent<RectTransform>();
                     cursorObj.gameObject.SetActive(false); // Hide initially, cursor script handles it
             }
        }
    }

    private GameAssetRegistry Registry
        {
            get
            {
                if (_registry == null)
                {
                    var initializer = FindFirstObjectByType<ArenaSceneInitializer>();
                    if (initializer != null)
                    {
                        _registry = initializer.registry;
                    }
                }
                return _registry;
            }
        }

        private void InitializeCanvasGroup()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
        }
        
        private void Update()
        {
            if (gameObject.activeSelf && InputManager.GetButtonDown("Cancel"))
            {
                InputManager.ConsumeButton("Cancel");
                OnCancelClicked();
            }
        }

        public static void Show(
            ItemSelectionDialog dialog,
            Action<int> onItemSelected,
            Action onCancel = null)
        {
            if (dialog == null)
            {
                Debug.LogError("[ItemSelectionDialog] Dialog instance is null.");
                return;
            }

            dialog.ShowInternal(onItemSelected, onCancel);
        }

        private void ShowInternal(Action<int> onItemSelected, Action onCancel)
        {
            InitializeCanvasGroup();
            _onItemSelected = onItemSelected;
            _onCancel = onCancel;

            PopulateItemList();

            gameObject.SetActive(true);

            // Animation
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, animationDuration).SetUpdate(true);
            }

            if (dialogPanel != null)
            {
                dialogPanel.localScale = Vector3.one * 0.8f;
                dialogPanel.DOScale(Vector3.one, animationDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
            
            // Select first available item or cancel button
            if (_instantiatedRows.Count > 0)
            {
                 var btn = _instantiatedRows[0].GetComponentInChildren<Button>();
                 if (btn != null) EventSystem.current.SetSelectedGameObject(btn.gameObject);
            }
        }

        private void PopulateItemList()
        {
            ClearItemList();

            // Access GameState.Player.Inventory directly (or GameState.Battle.CurrentTurn.Actor.Inventory if specific)
            // Ideally we default to Player for now as this is a UI for the player
            var inventory = GameState.Player?.Inventory;
            
            if (inventory != null)
            {
                if (emptyMessage != null) emptyMessage.gameObject.SetActive(false);
                if (scrollView != null) scrollView.gameObject.SetActive(true);

                // Only create rows for items with quantity > 0
                // This ensures the cursor only appears next to populated rows
                foreach (var slot in inventory.Slots)
                {
                    if (slot.Quantity <= 0) continue;

                    var item = GameWorld.GetItemById(slot.ItemId);
                    if (item == null) continue;

                    CreateItemRow(item, slot.Quantity);
                }
            }
            
            // Allow empty message to show if no items, but we should always show "None"
            // Actually requirement says "replace cancel button with a terminal item", so we assume adding "None" row at end.
            if (_instantiatedRows.Count == 0 && emptyMessage != null)
            {
                 emptyMessage.gameObject.SetActive(true);
            }
            
            CreateNoneRow();
        }

        private void CreateItemRow(Item item, int quantity)
        {
            if (itemRowPrefab == null || contentContainer == null) return;

            var rowObj = Instantiate(itemRowPrefab, contentContainer);
            _instantiatedRows.Add(rowObj);

            var rowScript = rowObj.GetComponent<ItemRow>();
            if (rowScript != null)
            {
                Sprite iconSprite = null;
                if (Registry != null && !string.IsNullOrEmpty(item.Sprite))
                {
                    iconSprite = Registry.GetSprite(item.Sprite);
                } else {
                    Debug.Log("Registry = " + (Registry == null ? "null" : "not null"));
                    Debug.Log("Sprite = " + item.Sprite);
                    Debug.Log("No sprite found for item: " + item.Name);
                }

                rowScript.Initialize(item, quantity, iconSprite, () => OnItemClicked(item.Id));
            }
            
            // Ensure button handles selection for cursor
            // The Button component does this automatically if configured
            rowObj.SetActive(true);
        }

        private void CreateNoneRow()
        {
            if (itemRowPrefab == null || contentContainer == null) return;

            var rowObj = Instantiate(itemRowPrefab, contentContainer);
            _instantiatedRows.Add(rowObj);

            var rowScript = rowObj.GetComponent<ItemRow>();
            if (rowScript != null)
            {
                rowScript.InitializeCustom("None", OnCancelClicked);
            }
            rowObj.SetActive(true);
        }

        private void ClearItemList()
        {
            foreach (var row in _instantiatedRows)
            {
                Destroy(row);
            }
            _instantiatedRows.Clear();
        }

        private void OnItemClicked(int itemId)
        {
             if (selectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(selectSound);
            }

            Hide(immediate: false, onComplete: () =>
            {
                _onItemSelected?.Invoke(itemId);
                _onItemSelected = null;
                _onCancel = null;
            });
        }

        private void OnCancelClicked()
        {
            if (cancelSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(cancelSound);
            }

            Hide(immediate: false, onComplete: () =>
            {
                _onCancel?.Invoke();
                _onItemSelected = null;
                _onCancel = null;
            });
        }

        private void Hide(bool immediate, Action onComplete = null)
        {
             if (immediate)
            {
                if (_canvasGroup != null) _canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                onComplete?.Invoke();
                return;
            }

            Sequence hideSequence = DOTween.Sequence();
            
            if (_canvasGroup != null)
            {
                hideSequence.Join(_canvasGroup.DOFade(0f, animationDuration).SetUpdate(true));
            }

            if (dialogPanel != null)
            {
                hideSequence.Join(dialogPanel.DOScale(Vector3.one * 0.8f, animationDuration)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true));
            }

            hideSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }
    }
}
