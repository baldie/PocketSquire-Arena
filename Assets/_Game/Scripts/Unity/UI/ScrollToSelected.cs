using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace PocketSquire.Unity.UI
{
    /// <summary>
    /// Forces a ScrollRect to scroll such that the currently selected UI element
    /// (e.g. via keyboard or gamepad) remains visible within the viewport.
    /// Place this script on the same GameObject as the ScrollRect.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollToSelected : MonoBehaviour
    {
        private ScrollRect _scrollRect;
        private RectTransform _contentRect;
        private RectTransform _viewportRect;
        private GameObject _lastSelected;

        [Tooltip("The amount of padding (in units) to keep between the selected item and the edge of the viewport.")]
        public float padding = 10f;

        [Tooltip("How fast to smoothly scroll to the selected item. If 0, snaps instantly.")]
        public float scrollSpeed = 2f;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            if (_scrollRect != null)
            {
                _contentRect = _scrollRect.content;
                _viewportRect = _scrollRect.viewport;
                
                // If there's no explicitly assigned viewport, the ScrollRect uses its own rect transform.
                if (_viewportRect == null)
                {
                    _viewportRect = GetComponent<RectTransform>();
                }
            }
        }

        private Coroutine _scrollCoroutine;

        private void Update()
        {
            if (_scrollRect == null || _contentRect == null || _viewportRect == null) return;
            if (EventSystem.current == null) return;

            var currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected == null) return;

            // Only act if the selection actually changed to a new element inside our content
            if (currentSelected != _lastSelected)
            {
                _lastSelected = currentSelected;

                // Check if the newly selected object is a child of our scroll content
                var targetRect = currentSelected.GetComponent<RectTransform>();
                if (targetRect != null && targetRect.IsChildOf(_contentRect))
                {
                    if (_scrollCoroutine != null)
                    {
                        StopCoroutine(_scrollCoroutine);
                    }
                    _scrollCoroutine = StartCoroutine(ScrollToRect(targetRect));
                }
            }
        }

        private IEnumerator ScrollToRect(RectTransform target)
        {
            // Wait for layout to settle
            yield return new WaitForEndOfFrame();

            // Calculate bounding box of the selected item relative to the Viewport
            Vector3[] targetCorners = new Vector3[4];
            target.GetWorldCorners(targetCorners);

            for (int i = 0; i < 4; i++)
            {
                targetCorners[i] = _viewportRect.InverseTransformPoint(targetCorners[i]);
            }

            // viewport corners: 0=bottomLeft, 1=topLeft, 2=topRight, 3=bottomRight
            // in local space of viewport, the rect represents the bounds
            float viewportTop = _viewportRect.rect.yMax;
            float viewportBottom = _viewportRect.rect.yMin;

            float targetTop = targetCorners[1].y;
            float targetBottom = targetCorners[0].y;

            // Calculate the required vertical shift
            float shiftAmountY = 0f;

            if (targetTop > viewportTop - padding)
            {
                // Item is above viewport, scroll up
                shiftAmountY = targetTop - (viewportTop - padding);
            }
            else if (targetBottom < viewportBottom + padding)
            {
                // Item is below viewport, scroll down
                shiftAmountY = targetBottom - (viewportBottom + padding);
            }

            if (Mathf.Abs(shiftAmountY) > 0.01f)
            {
                float newContentY = _contentRect.anchoredPosition.y - shiftAmountY;
                
                if (scrollSpeed > 0f)
                {
                    _scrollRect.velocity = Vector2.zero; // Stop inertia
                    float startY = _contentRect.anchoredPosition.y;
                    float time = 0f;
                    while (time < 1f)
                    {
                        time += Time.unscaledDeltaTime * scrollSpeed;
                        _contentRect.anchoredPosition = new Vector2(
                            _contentRect.anchoredPosition.x, 
                            Mathf.Lerp(startY, newContentY, time)
                        );
                        yield return null;
                    }
                }
                
                // Snap to final to be safe
                _contentRect.anchoredPosition = new Vector2(_contentRect.anchoredPosition.x, newContentY);
                _scrollRect.velocity = Vector2.zero;
            }
        }
    }
}
