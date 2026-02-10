using UnityEngine;

namespace PocketSquire.Unity.UI
{
    public class MenuCursorTarget : MonoBehaviour
    {
        [Tooltip("Offset from the object's position where the cursor should be placed.")]
        public Vector3 cursorOffset;
        
        [Tooltip("If true, the offset is applied relative to the object's local rotation/scale (TransformPoint). If false, it's just added to world position.")]
        public bool useLocalOffset = true;
    }
}
