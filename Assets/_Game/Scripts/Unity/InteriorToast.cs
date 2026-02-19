using UnityEngine;
using TMPro;
using DG.Tweening;

public class InteriorToast : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI toastText;
    private Tween toastTween;

    void Start()
    {
        // Start invisible
        toastText.alpha = 0;
    }

    public void ShowToast(string message, float duration = 2f)
    {
        // 1. Kill any active tween to prevent overlapping animations
        toastTween?.Kill();

        // 2. Set the text
        toastText.text = message;

        // 3. Create a DOTween Sequence
        toastTween = DOTween.Sequence()
            .Append(toastText.DOFade(1, 0.2f))        // Fade in (0.2s)
            .AppendInterval(duration)                 // Wait
            .Append(toastText.DOFade(0, 0.5f))        // Fade out (0.5s)
            .OnComplete(() => toastText.text = "");   // Clear text at the end
    }
}