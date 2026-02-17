using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using PocketSquire.Arena.Core.PowerUps;

public class PowerUpSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Assignments")]
    [Tooltip("The text component to display the description. If null, will attempt to find 'PowerUpDetails' in scene.")]
    public TextMeshProUGUI descriptionText;

    private PowerUp _powerUp;
    public PowerUp PowerUp => _powerUp;

    private void OnDisable()
    {
        if (descriptionText != null)
        {
            descriptionText.text = " ";
        }
    }

    private void Start()
    {
        if (descriptionText == null)
        {
            var go = GameObject.Find("PowerUpDetails");
            if (go != null)
            {
                descriptionText = go.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    public void Initialize(PowerUp powerUp)
    {
        _powerUp = powerUp;
    }

    // --- MOUSE LOGIC ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_powerUp != null && descriptionText != null)
        {
            descriptionText.text = _powerUp.Description;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (descriptionText != null)
        {
            descriptionText.text = " ";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (descriptionText != null)
        {
            descriptionText.text = " ";
        }
    }
}
