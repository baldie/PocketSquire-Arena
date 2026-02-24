using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Unity.UI
{
    /// <summary>
    /// A shop row that can display any IMerchandise — both Items and Perks.
    /// Replaces the old ItemRow for all shop display purposes.
    /// </summary>
    public class MerchandiseRow : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button button;
        [SerializeField] private EventTrigger eventTrigger;

        public Action<IMerchandise> OnSelected;
        private IMerchandise currentMerchandise;

        private void Awake()
        {
            if (icon == null) icon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (nameText == null) nameText = transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
            if (quantityText == null) quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            if (priceText == null) priceText = transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            if (button == null) button = GetComponent<Button>();
            if (eventTrigger == null) eventTrigger = GetComponent<EventTrigger>();
        }

        /// <summary>
        /// Initialize the row with any IMerchandise (Item or Perk).
        /// </summary>
        public void Initialize(IMerchandise merchandise, Sprite merchandiseIcon, Action onClick, bool showPrice = true)
        {
            if (merchandise == null) return;
            currentMerchandise = merchandise;

            if (nameText != null) nameText.text = merchandise.DisplayName;
            if (descriptionText != null) descriptionText.text = merchandise.Description;

            if (quantityText != null)
            {
                // Quantity not applicable for generic merchandise — hide by default
                quantityText.gameObject.SetActive(false);
            }

            if (priceText != null)
            {
                priceText.gameObject.SetActive(showPrice);
                priceText.text = $"{merchandise.Price}";
            }

            if (icon != null)
            {
                icon.gameObject.SetActive(true);
                if (merchandiseIcon != null)
                {
                    icon.sprite = merchandiseIcon;
                    icon.color = Color.white;
                }
                else
                {
                    icon.sprite = null;
                    icon.color = Color.clear;
                }
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }

        /// <summary>
        /// Legacy: initialize with a quantity label (e.g. "x2") for stacked items.
        /// </summary>
        public void SetQuantity(int quantity)
        {
            if (quantityText == null) return;
            if (quantity > 1)
            {
                quantityText.gameObject.SetActive(true);
                quantityText.text = $"x{quantity}";
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }

        public void HidePriceText()
        {
            if (priceText != null) priceText.gameObject.SetActive(false);
        }

        public void InitializeCustom(string title, Action onClick)
        {
            currentMerchandise = null;
            if (nameText != null) nameText.text = title;
            if (descriptionText != null) descriptionText.text = "";
            if (quantityText != null) quantityText.gameObject.SetActive(false);
            if (priceText != null) priceText.gameObject.SetActive(false);
            if (icon != null) icon.gameObject.SetActive(false);

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (currentMerchandise != null)
            {
                OnSelected?.Invoke(currentMerchandise);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (button == null) button = GetComponent<Button>();
            if (eventTrigger == null) eventTrigger = GetComponent<EventTrigger>();

            if (button != null && eventTrigger != null)
            {
                bool hasPointerEnter = false;
                foreach (var entry in eventTrigger.triggers)
                {
                    if (entry.eventID == EventTriggerType.PointerEnter)
                    {
                        hasPointerEnter = true;
                        break;
                    }
                }

                if (!hasPointerEnter)
                {
                    var entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerEnter;
                    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                        entry.callback,
                        button.Select
                    );
                    eventTrigger.triggers.Add(entry);
                    UnityEditor.EditorUtility.SetDirty(this);
                    UnityEditor.EditorUtility.SetDirty(eventTrigger);
                }
            }
        }
#endif
    }
}
