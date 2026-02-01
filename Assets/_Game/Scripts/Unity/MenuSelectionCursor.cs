using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuSelectionCursor : MonoBehaviour
{
    [Header("Setup")]
    public RectTransform cursorGraph; // Drag your Cursor Image here
    public float moveSpeed = 15f;     // How snappy the cursor moves (higher is faster)

    void Update()
    {
        // 1. Get the currently selected object from the Event System
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        if (cursorGraph == null) return;

        // 2. Check if something is actually selected
        if (selectedObj != null)
        {
            // Check if it's a button AND if that button is interactable
            Button btn = selectedObj.GetComponent<Button>();
            if(btn != null && btn.interactable) 
            {
                cursorGraph.gameObject.SetActive(true);

                // 3. Define the target position
                // We use the button's Y position, but keep our own fixed X offset relative to the button
                Vector3 targetPosition = selectedObj.transform.position;
                targetPosition.x = cursorGraph.position.x;

                // 4. Move smoothly to that position
                // (Use Vector3.MoveTowards for linear speed, or Vector3.Lerp for "ease-in" feel)
                cursorGraph.position = Vector3.Lerp(cursorGraph.position, targetPosition, moveSpeed * Time.unscaledDeltaTime);
            }
            else
            {
                // Hide the cursor if the button is disabled (interactable = false)
                cursorGraph.gameObject.SetActive(false);
            }
        }
        else
        {
            // Optional: Hide cursor if nothing is selected (e.g. user clicked empty space)
            cursorGraph.gameObject.SetActive(false);
        }
    }
}