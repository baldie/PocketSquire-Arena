using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using PocketSquire.Arena.Core.PowerUps;

public class PowerUpButtonScript : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage; // Placeholder for now
    public Button selectButton;

    private PowerUp _powerUp;
    private Action<PowerUp> _onSelected;

    void Awake()
    {
        if (selectButton == null) selectButton = GetComponent<Button>();
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnButtonClicked);
        }
    }

    public void LoadPowerUp(PowerUp powerUp, Action<PowerUp> onSelected)
    {
        _powerUp = powerUp;
        _onSelected = onSelected;

        if (nameText != null)
        {
            nameText.text = powerUp.DisplayName;
            
            // Apply rarity-based color tint
            nameText.color = powerUp.Component.Rarity switch
            {
                Rarity.Rare => new Color(0.3f, 0.5f, 1f),        // Blue
                Rarity.Epic => new Color(0.7f, 0.3f, 1f),        // Purple
                Rarity.Legendary => new Color(1f, 0.6f, 0f),     // Orange
                _ => Color.white                                  // Common (white)
            };
        }
        
        if (descriptionText != null) descriptionText.text = powerUp.Description;
        
        // Load icon from GameAssetRegistry
        if (iconImage != null)
        {
            var sprite = GameAssetRegistry.Instance.GetSprite(powerUp.Component.IconId);
            if (sprite != null)
            {
                iconImage.sprite = sprite;
            }
            else
            {
                GameAssetRegistry.Instance.LogAllSprites();
                Debug.LogWarning($"[PowerUpButtonScript] Icon sprite '{powerUp.Component.IconId}' not found in GameAssetRegistry");
            }
        }
    }

    private void OnButtonClicked()
    {
        if (_powerUp != null)
        {
            _onSelected?.Invoke(_powerUp);
        }
    }
}
