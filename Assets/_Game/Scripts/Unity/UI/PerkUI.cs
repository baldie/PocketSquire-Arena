using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using PocketSquire.Arena.Core.Perks;
using PocketSquire.Arena.Unity;

namespace PocketSquire.Arena.Unity.UI
{
    public class PerkUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image perkIcon;
        public TextMeshProUGUI perkDescriptionText;

        private ArenaPerk _loadedPerk;

        private void Awake()
        {
            if (perkIcon == null)
            {
                // Try to find the child named "PerkIcon" if not assigned
                var iconTransform = transform.Find("PerkIcon");
                if (iconTransform != null)
                {
                    perkIcon = iconTransform.GetComponent<Image>();
                }
            }
        }

        public void LoadPerk(ArenaPerk perk)
        {
            _loadedPerk = perk;
            if (_loadedPerk != null && perkIcon != null)
            {
                if (!string.IsNullOrEmpty(_loadedPerk.Icon))
                {
                    // Assuming GameAssetRegistry handles sprite loading by name
                    Sprite sprite = GameAssetRegistry.Instance.GetSprite(_loadedPerk.Icon);
                    if (sprite != null)
                    {
                        perkIcon.sprite = sprite;
                        perkIcon.color = Color.white;
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_loadedPerk != null && perkDescriptionText != null)
            {
                perkDescriptionText.text = _loadedPerk.Description;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (perkDescriptionText != null)
            {
                perkDescriptionText.text = "";
            }
        }
    }
}
