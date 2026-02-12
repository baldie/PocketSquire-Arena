using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Unity.UI
{
    public class ItemRow : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button button;
        [SerializeField] private EventTrigger eventTrigger;

        public Action<Item> OnSelected;
        private Item currentItem;

        private void Awake()
        {
            if (icon == null) icon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (nameText == null) nameText = transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
            if (quantityText == null) quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            if (priceText == null) priceText = transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            if (button == null) button = GetComponent<Button>();
            if (eventTrigger == null) eventTrigger = GetComponent<EventTrigger>();
        }

        public void Initialize(Item item, int quantity, Sprite itemSprite, Action onClick)
        {
            if (item == null) return;
            currentItem = item;

            if (nameText != null) nameText.text = item.Name;
            if (descriptionText != null) descriptionText.text = item.Description;
            
            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(true);
                quantityText.text = quantity > 1 ? $"x{quantity}" : "";
            }

            if (priceText != null)
            {
                priceText.gameObject.SetActive(true);
                priceText.text = $"{item.Price}";
            }

            if (icon != null)
            {
                icon.gameObject.SetActive(true);
                if (itemSprite != null)
                {
                    icon.sprite = itemSprite;
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

        public void OnSelect(BaseEventData eventData)
        {
            if (currentItem != null)
            {
                OnSelected?.Invoke(currentItem);
            }
        }

        public void InitializeCustom(string title, Action onClick)
        {
            currentItem = null;
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
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            if (button == null) button = GetComponent<Button>();
            if (eventTrigger == null) eventTrigger = GetComponent<EventTrigger>();
            
            if (button != null && eventTrigger != null)
            {
                bool hasPointerEnter = false;
                foreach(var entry in eventTrigger.triggers)
                {
                    if(entry.eventID == EventTriggerType.PointerEnter)
                    {
                        hasPointerEnter = true;
                        break;
                    }
                }
                
                if (!hasPointerEnter)
                {
                    var entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerEnter;
                    
                    // We need to use UnityEventTools to make it persistent in Editor
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
