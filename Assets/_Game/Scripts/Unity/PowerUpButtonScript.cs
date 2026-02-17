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
    public Transform iconPlaceholder;
    public Button selectButton;

    [Header("Prefab References")]
    public GameObject powerUpIconPrefab;

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
            
            // Apply rarity-based color tint using shared helper
            nameText.color = PowerUpIconController.GetRarityColor(powerUp.Component.Rarity);
        }
        
        if (descriptionText != null) descriptionText.text = powerUp.Description;
        
        // Instantiate PowerUpIcon prefab into placeholder
        if (iconPlaceholder != null && powerUpIconPrefab != null)
        {
            // Clear any existing icon
            foreach (Transform child in iconPlaceholder)
            {
                Destroy(child.gameObject);
            }

            // Create new icon using shared method
            var iconObj = PowerUpHudController.CreatePowerUpIcon(powerUpIconPrefab, powerUp, iconPlaceholder);
            
            // Make the icon clickable - clicking it should trigger the parent button
            if (iconObj != null && selectButton != null)
            {
                var iconButton = iconObj.GetComponent<Button>();
                if (iconButton == null)
                {
                    iconButton = iconObj.AddComponent<Button>();
                    iconButton.transition = Selectable.Transition.None; // No visual feedback on the icon itself
                }
                
                // Forward clicks on the icon to the parent button
                iconButton.onClick.AddListener(() => selectButton.onClick.Invoke());
            }
        }
        else if (iconPlaceholder == null)
        {
            Debug.LogWarning("[PowerUpButtonScript] iconPlaceholder is not assigned");
        }
        else if (powerUpIconPrefab == null)
        {
            Debug.LogWarning("[PowerUpButtonScript] powerUpIconPrefab is not assigned");
        }

        // Initialize PowerUpSelector
        var selector = GetComponent<PowerUpSelector>();
        if (selector == null)
        {
            selector = gameObject.AddComponent<PowerUpSelector>();
        }
        selector.Initialize(powerUp);
    }

    private void OnButtonClicked()
    {
        if (_powerUp != null)
        {
            _onSelected?.Invoke(_powerUp);
        }
    }
}
