using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PocketSquire.Arena.Core;

namespace PocketSquire.Arena.Unity.UI
{
    public class ItemRow : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Button button;
        [SerializeField] private EventTrigger eventTrigger;

        private void Awake()
        {
            if (icon == null) icon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (nameText == null) nameText = transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
            if (quantityText == null) quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            if (button == null) button = GetComponent<Button>();
            if (eventTrigger == null) eventTrigger = GetComponent<EventTrigger>();
        }

        public void Initialize(Item item, int quantity, Sprite itemSprite, Action onClick)
        {
            if (item == null) return;

            if (nameText != null) nameText.text = item.Name;
            if (descriptionText != null) descriptionText.text = item.Description;
            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(true);
                quantityText.text = $"x{quantity}";
            }

            if (icon != null)
            {
                icon.gameObject.SetActive(true);
                if (itemSprite != null) icon.sprite = itemSprite;
            }
            
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }

        public void InitializeCustom(string title, Action onClick)
        {
            if (nameText != null) nameText.text = title;
            if (descriptionText != null) descriptionText.text = "";
            if (quantityText != null) quantityText.gameObject.SetActive(false);
            if (icon != null) icon.gameObject.SetActive(false); // Hide icon for custom rows

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }
    }
}
