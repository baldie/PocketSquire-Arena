using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PocketSquire.Unity
{
    /// <summary>
    /// Singleton input manager that tracks consumed input per frame.
    /// Prevents multiple systems from responding to the same input event.
    /// Uses Unity Input System (com.unity.inputsystem) via PlayerInput component.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        private HashSet<string> _consumedInputsThisFrame = new HashSet<string>();
        private PlayerInput _playerInput;

        public static InputManager Instance => _instance;
        public static PlayerInput PlayerInput => _instance != null ? _instance._playerInput : null;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _playerInput = GetComponent<PlayerInput>();
            
            if (_playerInput == null)
            {
                Debug.LogError("[InputManager] PlayerInput component missing! usage will fail.");
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
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
            if (_instance == null || _instance._playerInput == null) return false;

            // Debug.Log($"GetButtonDown: {buttonName}"); // Commented out to reduce spam
            
            if (_instance._consumedInputsThisFrame.Contains(buttonName))
                return false;
            
            // Access action dynamically by name from PlayerInput
            // Note: This requires the Actions asset to have actions with these exact names.
            InputAction action = _instance._playerInput.actions[buttonName];
            
            if (action == null)
            {
                Debug.LogWarning($"[InputManager] Unknown action requested: {buttonName}");
                return false;
            }

            if (action.WasPressedThisFrame())
            {
                 Debug.Log($"{buttonName} button pressed");
                 return true;
            }
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
