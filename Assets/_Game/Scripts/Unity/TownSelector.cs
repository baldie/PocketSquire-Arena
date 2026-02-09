using UnityEngine;
using UnityEngine.UI; // Required for RawImage
using UnityEngine.EventSystems;
using TMPro;

public class TownSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Assignments")]
    public RawImage backgroundRenderer; 
    public Texture highlightedTexture;
    public TextMeshProUGUI nameText;
    public string locationDisplayName;

    private Texture defaultTexture;

    void Awake()
    {
        // Capture the "normal" town texture
        if (backgroundRenderer != null)
        {
            defaultTexture = backgroundRenderer.texture;
        }
    }

    // --- MOUSE LOGIC ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        SwapBackground(highlightedTexture);
        SetLocationText(locationDisplayName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SwapBackground(defaultTexture);
        SetLocationText(" ");
    }

    // --- GAMEPAD LOGIC ---
    public void OnSelect(BaseEventData eventData)
    {
        SwapBackground(highlightedTexture);
        SetLocationText(locationDisplayName);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SwapBackground(defaultTexture);
        SetLocationText(" ");
    }

    // Helper method
    void SwapBackground(Texture newTexture)
    {
        if (backgroundRenderer != null && newTexture != null)
        {
            backgroundRenderer.texture = newTexture;
        }
    }

    void SetLocationText(string displayName)
    {
        if (nameText != null)
        {
            nameText.text = displayName;
        }
    }
}