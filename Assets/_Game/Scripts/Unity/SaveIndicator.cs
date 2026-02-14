using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace PocketSquire.Unity
{
    public class SaveIndicator : MonoBehaviour
    {
        [SerializeField] private Image _indicatorImage;
        [SerializeField] private float _spinSpeed = 360f; // Degrees per second

        private Tweener _spinTween;

        private void Awake()
        {
            Debug.Log("[SaveIndicator] Awake");
            if (_indicatorImage == null)
            {
                _indicatorImage = GetComponentInChildren<Image>();
            }

            // Ensure starts hidden
            SetVisible(false);
        }

        private void OnEnable()
        {
            Debug.Log("[SaveIndicator] OnEnable - Subscribing to SaveSystem events");
            SaveSystem.OnSaveStarted += HandleSaveStarted;
            SaveSystem.OnSaveEnded += HandleSaveEnded;
        }

        private void OnDisable()
        {
            Debug.Log("[SaveIndicator] OnDisable - Unsubscribing from SaveSystem events");
            SaveSystem.OnSaveStarted -= HandleSaveStarted;
            SaveSystem.OnSaveEnded -= HandleSaveEnded;
            StopSpin();
        }

        private void HandleSaveStarted()
        {
            Debug.Log("[SaveIndicator] HandleSaveStarted received");
            SetVisible(true);
            StartSpin();
        }

        private void HandleSaveEnded()
        {
            Debug.Log("[SaveIndicator] HandleSaveEnded received");
            
            // Add a small delay so it doesn't flicker too fast if save is instant
            DOVirtual.DelayedCall(0.5f, () => {
                SetVisible(false);
                StopSpin();
            });
        }

        private void SetVisible(bool visible)
        {
            Debug.Log($"[SaveIndicator] SetVisible: {visible}");
            if (_indicatorImage != null)
            {
                _indicatorImage.enabled = visible;
            }
            else
            {
                Debug.LogWarning("[SaveIndicator] _indicatorImage is null!");
            }
        }

        private void StartSpin()
        {
            if (_spinTween != null && _spinTween.IsActive()) return;

            if (_indicatorImage != null)
            {
                // Calculate duration based on speed (degrees per second)
                float duration = 360f / _spinSpeed;
                _spinTween = _indicatorImage.transform
                    .DORotate(new Vector3(0, 0, -360), duration, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart);
            }
        }

        private void StopSpin()
        {
            if (_spinTween != null)
            {
                _spinTween.Kill();
                _spinTween = null;
            }
        }
    }
}
