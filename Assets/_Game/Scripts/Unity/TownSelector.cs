using UnityEngine;
using UnityEngine.UI; // Required for RawImage
using UnityEngine.EventSystems;

public class TownSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Assignments")]
    // CHANGED: We now reference a RawImage instead of a standard Image
    public RawImage backgroundRenderer; 
    
    // CHANGED: We now hold a Texture (the raw file) instead of a Sprite
    public Texture highlightedTexture;  

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
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SwapBackground(defaultTexture);
    }

    // --- GAMEPAD LOGIC ---
    public void OnSelect(BaseEventData eventData)
    {
        SwapBackground(highlightedTexture);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SwapBackground(defaultTexture);
    }

    // Helper method
    void SwapBackground(Texture newTexture)
    {
        if (backgroundRenderer != null && newTexture != null)
        {
            backgroundRenderer.texture = newTexture;
        }
    }
}