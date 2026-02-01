using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PocketSquire.Unity
{
    /// <summary>
    /// Singleton input manager that tracks consumed input per frame.
    /// Prevents multiple systems from responding to the same input event.
    /// Uses Unity Input System (com.unity.inputsystem).
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        private HashSet<string> _consumedInputsThisFrame = new HashSet<string>();

        private InputAction _pauseAction;
        private InputAction _cancelAction;

        public static InputManager Instance => _instance;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            InitializeActions();
        }

        private void InitializeActions()
        {
            // Define Pause Action (Escape or Gamepad Start)
            _pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");
            _pauseAction.AddBinding("<Gamepad>/start");

            // Define Cancel Action (Escape or Gamepad Button East/B)
            _cancelAction = new InputAction("Cancel", binding: "<Keyboard>/escape");
            _cancelAction.AddBinding("<Gamepad>/buttonEast");
        }

        void OnEnable()
        {
            _pauseAction?.Enable();
            _cancelAction?.Enable();
        }

        void OnDisable()
        {
            _pauseAction?.Disable();
            _cancelAction?.Disable();
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
            if (_instance == null) return false;
            
            if (_instance._consumedInputsThisFrame.Contains(buttonName))
                return false;
            
            // Map legacy string names to Input Actions
            if (buttonName == "Pause")
            {
                return _instance._pauseAction.WasPressedThisFrame();
            }
            else if (buttonName == "Cancel")
            {
                return _instance._cancelAction.WasPressedThisFrame();
            }

            Debug.LogWarning($"[InputManager] Unknown button requested: {buttonName}");
            return false;
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
