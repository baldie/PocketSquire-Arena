using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PocketSquire.Unity.UI;

public class MenuSelectionCursor : MonoBehaviour
{
    [Header("Setup")]
    public RectTransform cursorGraph; // Drag your Cursor Image here
    public float moveSpeed = 15f;     // How snappy the cursor moves (higher is faster)
    public Vector3 defaultCursorOffset;

    private GameObject _lastSelected;
    private MenuCursorTarget _cachedTarget;
    private Image _cursorImage;

    private void Awake()
    {
        if (cursorGraph) _cursorImage = cursorGraph.GetComponent<Image>();
    }

    private void Update()
    {
        if (cursorGraph == null) return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (ValidateSelection(selected))
        {
            MoveCursorTo(selected);
        }
        else
        {
            SetVisible(false);
            _lastSelected = null; // Reset cache so we re-validate if selected again
        }
    }

    private bool ValidateSelection(GameObject selection)
    {
        if (selection == null) return false;

        // 1. Ownership Check: Ensure this cursor instance is responsible for the selected object
        // Find the nearest MenuSelectionCursor in the selection's hierarchy
        var owner = selection.GetComponentInParent<MenuSelectionCursor>();
        
        // Only valid if WE are that cursor
        if (owner != this) return false;

        // 2. Interactability Check
        var btn = selection.GetComponent<Button>();
        return btn != null && btn.interactable;
    }

    private void MoveCursorTo(GameObject target)
    {
        SetVisible(true);

        // Cache target component only when selection changes
        if (target != _lastSelected)
        {
            _lastSelected = target;
            _cachedTarget = target.GetComponent<MenuCursorTarget>();
        }

        // Calculate Target Position
        Vector3 targetPos = target.transform.position;

        if (_cachedTarget != null)
        {
            // Apply Custom Offset
            targetPos += _cachedTarget.useLocalOffset 
                ? target.transform.TransformVector(_cachedTarget.cursorOffset)
                : _cachedTarget.cursorOffset;
        }
        else
        {
            // Apply Default Offset
            targetPos += defaultCursorOffset;
        }

        // Move Smoothly
        cursorGraph.position = Vector3.Lerp(cursorGraph.position, targetPos, moveSpeed * Time.unscaledDeltaTime);
    }

    private void SetVisible(bool visible)
    {
        // Safe toggle that won't disable this script if valid
        if (_cursorImage) 
        {
            _cursorImage.enabled = visible;
        }
        else if (cursorGraph.gameObject != gameObject) 
        {
            cursorGraph.gameObject.SetActive(visible);
        }
    }
}