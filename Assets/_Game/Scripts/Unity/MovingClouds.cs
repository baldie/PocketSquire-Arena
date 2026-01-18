using UnityEngine;
using UnityEngine.UI;

public class AutoScrollTexture : MonoBehaviour
{
    // Speed of the wind (X = horizontal, Y = vertical)
    // Try small numbers like 0.05 or 0.1
    public Vector2 windSpeed = new Vector2(0.1f, 0f);
    
    private RawImage _rawImage;

    void Start()
    {
        _rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        // Get the current rectangle
        Rect uvRect = _rawImage.uvRect;

        // Shift the position based on time and speed
        uvRect.x += windSpeed.x * Time.deltaTime;
        uvRect.y += windSpeed.y * Time.deltaTime;

        // Apply the new rectangle back to the image
        _rawImage.uvRect = uvRect;
    }
}