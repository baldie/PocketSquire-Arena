using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PocketSquire.Unity.UI;

public class MenuSelectionCursor : MonoBehaviour
{
    [Header("Setup")]
    public RectTransform cursorGraph; // Drag your Cursor Image here
    public float moveSpeed = 15f;     // How snappy the cursor moves (higher is faster)

    private GameObject _lastSelectedObj;
    private MenuCursorTarget _cachedTarget;

    void Update()
    {
        // 1. Get the currently selected object from the Event System
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        if (cursorGraph == null) return;

        // 2. Check if something is actually selected
        if (selectedObj != null)
        {
            // Update cache if selection changed
            if (selectedObj != _lastSelectedObj)
            {
                _lastSelectedObj = selectedObj;
                _cachedTarget = selectedObj.GetComponent<MenuCursorTarget>();
            }

            // Check if it's a button AND if that button is interactable
            // Optimization: We could also cache the Button component if this proves heavy, 
            // but GetComponent<Button> is very cheap.
            Button btn = selectedObj.GetComponent<Button>();
            if(btn != null && btn.interactable) 
            {
                cursorGraph.gameObject.SetActive(true);

                // 3. Define the target position
                Vector3 targetPosition = selectedObj.transform.position;
                
                if (_cachedTarget != null)
                {
                    // Use the configured offset
                    if (_cachedTarget.useLocalOffset)
                    {
                        targetPosition += selectedObj.transform.TransformVector(_cachedTarget.cursorOffset);
                    }
                    else
                    {
                        // World space offset
                        targetPosition += _cachedTarget.cursorOffset;
                    }
                }
                else
                {
                    // Legacy behavior: lock X
                    targetPosition.x = cursorGraph.position.x;
                }

                // 4. Move smoothly to that position
                cursorGraph.position = Vector3.Lerp(cursorGraph.position, targetPosition, moveSpeed * Time.unscaledDeltaTime);
            }
            else
            {
                cursorGraph.gameObject.SetActive(false);
            }
        }
        else
        {
            _lastSelectedObj = null;
            _cachedTarget = null;
            cursorGraph.gameObject.SetActive(false);
        }
    }
}