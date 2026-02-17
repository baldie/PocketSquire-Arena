using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PocketSquire.Unity
{
    /// <summary>
    /// Singleton input manager that tracks consumed input per frame.
    /// Wraps Unity Input System (com.unity.inputsystem) via PlayerInput component.
    /// Replaces the legacy InputManager.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(PlayerInput))]
    public class GameInput : MonoBehaviour
    {
        private static GameInput _instance;
        private HashSet<InputAction> _consumedActions = new HashSet<InputAction>();
        private PlayerInput _playerInput;

        public static GameInput Instance => _instance;
        public static PlayerInput PlayerInput => _instance != null ? _instance._playerInput : null;

        // Strongly typed actions
        public InputAction PauseAction { get; private set; }
        public InputAction CancelAction { get; private set; }
        public InputAction InventoryAction { get; private set; }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            transform.SetParent(null); // Ensure it's at root level for DontDestroyOnLoad
            DontDestroyOnLoad(gameObject);

            _playerInput = GetComponent<PlayerInput>();
            
            if (_playerInput == null)
            {
                Debug.LogError("[GameInput] PlayerInput component missing! usage will fail.");
                return;
            }

            // Cache actions
            if (_playerInput.actions == null)
            {
                Debug.LogError("[GameInput] PlayerInput actions asset is missing!");
                return;
            }

            PauseAction = _playerInput.actions["Pause"];
            CancelAction = _playerInput.actions["Cancel"];
            InventoryAction = _playerInput.actions["Inventory"];
        }

        private void OnEnable()
        {
            if (PauseAction != null && !PauseAction.enabled)
                PauseAction.Enable();
            
            if (CancelAction != null && !CancelAction.enabled)
                CancelAction.Enable();
            
            if (InventoryAction != null && !InventoryAction.enabled)
                InventoryAction.Enable();
        }

        private void OnDisable()
        {
            PauseAction?.Disable();
            CancelAction?.Disable();
            InventoryAction?.Disable();
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
            _consumedActions.Clear();
        }

        /// <summary>
        /// Check if an action was pressed this frame and hasn't been consumed yet.
        /// </summary>
        /// <param name="action">The InputAction to check</param>
        /// <returns>True if button was pressed and not yet consumed</returns>
        public bool GetButtonDown(InputAction action)
        {
            if (action == null)
                return false;
            
            if (_consumedActions.Contains(action))
                return false;
            
            // Migration Guide: Input.GetButtonDown -> InputAction.WasPressedThisFrame()
            return action.WasPressedThisFrame();
        }

        /// <summary>
        /// Mark an action as consumed for this frame, preventing other systems from reading it.
        /// </summary>
        /// <param name="action">The InputAction to consume</param>
        public void ConsumeButton(InputAction action)
        {
            if (action != null)
                _consumedActions.Add(action);
        }
    }
}
