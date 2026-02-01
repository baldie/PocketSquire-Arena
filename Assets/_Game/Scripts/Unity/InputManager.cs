using System.Collections.Generic;
using UnityEngine;

namespace PocketSquire.Unity
{
    /// <summary>
    /// Singleton input manager that tracks consumed input per frame.
    /// Prevents multiple systems from responding to the same input event.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        private HashSet<string> _consumedInputsThisFrame = new HashSet<string>();

        public static InputManager Instance => _instance;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        void LateUpdate()
        {
            // Clear consumed inputs at the end of each frame
            _consumedInputsThisFrame.Clear();
        }

        /// <summary>
        /// Check if a button was pressed this frame and hasn't been consumed yet.
        /// </summary>
        /// <param name="buttonName">The button name (e.g., "Cancel", "Pause")</param>
        /// <returns>True if button was pressed and not yet consumed</returns>
        public static bool GetButtonDown(string buttonName)
        {
            if (_instance == null || _instance._consumedInputsThisFrame.Contains(buttonName))
                return false;
            
            return Input.GetButtonDown(buttonName);
        }

        /// <summary>
        /// Mark a button as consumed for this frame, preventing other systems from reading it.
        /// </summary>
        /// <param name="buttonName">The button name to consume</param>
        public static void ConsumeButton(string buttonName)
        {
            if (_instance != null)
                _instance._consumedInputsThisFrame.Add(buttonName);
        }
    }
}
