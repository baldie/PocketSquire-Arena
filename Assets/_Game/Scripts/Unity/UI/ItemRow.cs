using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Unity.UI
{
    public class ItemRow : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IDeselectHandler, IPointerExitHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button button;

        public TextMeshProUGUI descriptionTextTarget;

        // Injected at runtime by ShopController — shared across all rows.
        // When this row is selected, its merchandise description is displayed here.
        private InteriorToast descriptionToast;

        public Action<Item> OnSelected;
        public Action<IMerchandise> OnMerchandiseSelected;
        private Item currentItem;
        private IMerchandise currentMerchandise;

        private void Awake()
        {
            if (icon == null)        icon        = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (nameText == null)    nameText     = transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
            if (quantityText == null) quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            if (priceText == null)   priceText    = transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            if (button == null)      button       = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && button.interactable)
            {
                button.Select();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Clear selection natively if the mouse leaves, triggering OnDeselect
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
                EventSystem.current.SetSelectedGameObject(null);
        }

        /// <summary>
        /// Provide a reference to the shared InteriorToast so this row can display
        /// the item/perk description when selected.
        /// </summary>
        public void SetDescriptionToast(InteriorToast toast)
        {
            descriptionToast = toast;
        }

        public void Initialize(Item item, int quantity, Sprite itemSprite, Action onClick, bool showPrice = true)
        {
            if (item == null) return;
            currentItem = item;
            currentMerchandise = item;

            if (nameText != null) nameText.text = item.Name;

            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(true);
                quantityText.text = quantity > 1 ? $"x{quantity}" : "";
            }

            SetPriceAndIcon(item.Price, itemSprite, showPrice);
            SetButtonAction(onClick);
        }

        /// <summary>
        /// Initialize the row with any IMerchandise (e.g. a Perk).
        /// Quantity text is hidden since it doesn't apply to generic merchandise.
        /// </summary>
        public void Initialize(IMerchandise merchandise, Sprite merchandiseIcon, Action onClick, bool showPrice = true)
        {
            if (merchandise == null) return;
            currentItem = null;
            currentMerchandise = merchandise;

            if (nameText != null) nameText.text = merchandise.DisplayName;
            if (quantityText != null) quantityText.gameObject.SetActive(false);

            SetPriceAndIcon(merchandise.Price, merchandiseIcon, showPrice);
            SetButtonAction(onClick);
        }

        private void SetPriceAndIcon(int price, Sprite sprite, bool showPrice)
        {
            if (priceText != null)
            {
                priceText.gameObject.SetActive(showPrice);
                priceText.text = $"{price}";
            }

            if (icon != null)
            {
                icon.gameObject.SetActive(true);
                if (sprite != null)
                {
                    icon.sprite = sprite;
                    icon.color = Color.white;
                }
                else
                {
                    icon.sprite = null;
                    icon.color = Color.clear;
                }
            }
        }

        private void SetButtonAction(Action onClick)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (currentItem != null)
                OnSelected?.Invoke(currentItem);
            else if (currentMerchandise != null)
                OnMerchandiseSelected?.Invoke(currentMerchandise);

            // Show description in the shared toast panel
            string desc = currentMerchandise?.Description ?? "";
            if (descriptionToast != null && !string.IsNullOrEmpty(desc))
                descriptionToast.ShowDescription(desc);

            if (descriptionTextTarget != null)
                descriptionTextTarget.text = desc;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (descriptionTextTarget != null)
                descriptionTextTarget.text = string.Empty;
        }

        public void HidePriceText()
        {
            if (priceText != null) priceText.gameObject.SetActive(false);
        }

        public void InitializeCustom(string title, Action onClick)
        {
            currentItem = null;
            currentMerchandise = null;
            if (nameText != null) nameText.text = title;
            if (quantityText != null) quantityText.gameObject.SetActive(false);
            if (priceText != null) priceText.gameObject.SetActive(false);
            if (icon != null) icon.gameObject.SetActive(false);

            SetButtonAction(onClick);
        }
    }
}
