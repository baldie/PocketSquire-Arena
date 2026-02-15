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

        if (nameText != null) nameText.text = powerUp.DisplayName;
        if (descriptionText != null) descriptionText.text = powerUp.Description;
        
        // TODO: Set icon
    }

    private void OnButtonClicked()
    {
        if (_powerUp != null)
        {
            _onSelected?.Invoke(_powerUp);
        }
    }
}
