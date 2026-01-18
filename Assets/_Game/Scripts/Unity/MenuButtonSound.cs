using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonSound : MonoBehaviour, ISelectHandler, IPointerEnterHandler, ISubmitHandler, IPointerClickHandler
{
    [Header("Audio Source Reference")]
    public AudioSource source;

    [Header("Audio Clips")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    // NEW: Variable to track when the button turned on
    private float enableTime; 

    // NEW: When the button or menu appears, start the stopwatch
    private void OnEnable()
    {
        // specific to Realtime so it works even if the game is paused
        enableTime = Time.realtimeSinceStartup; 
    }

    public void OnSelect(BaseEventData eventData)
    {
        // NEW: The Check
        // If less than 0.15 seconds passed since the menu opened, ignore this sound.
        if (Time.realtimeSinceStartup - enableTime < 0.15f)
        {
            return;
        }

        PlayHover();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHover();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        PlayClick();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClick();
    }

    private void PlayHover()
    {
        if (source != null && hoverSound != null)
        {
            source.PlayOneShot(hoverSound);
        }
    }

    public void PlayClick()
    {
        if (source != null && clickSound != null)
        {
            source.PlayOneShot(clickSound);
        }
    }
}