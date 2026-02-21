using UnityEngine;
using UnityEngine.UI;

namespace PocketSquire.Unity.UI
{
    /// <summary>
    /// Controls a single skill-tree connector line between two nodes.
    /// Swaps between dormant and active sprites to show progression.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class NodeConnector : MonoBehaviour
    {
        public enum ConnectorState { Dormant, Activated }

        [Header("Configuration")]
        [Tooltip("The node that this connector leads to.")]
        public NodeController targetNode;

        [Header("Sprites")]
        [Tooltip("Sprite shown when the connection is active")]
        public Sprite activeSprite;

        [Tooltip("Sprite shown when the connection is dormant (default)")]
        public Sprite dormantSprite;

        [Header("States")]
        public ConnectorState initialState = ConnectorState.Dormant;

        private Image _image;
        private ConnectorState _currentState = ConnectorState.Dormant;

        public ConnectorState CurrentState => _currentState;

        private void Awake()
        {
            if (!TryGetComponent(out _image)) Debug.LogError($"{name}: Missing Image component!");
        }

        private void Start()
        {
            if (initialState == ConnectorState.Activated)
            {
                Activate();
            }
            else
            {
                ApplyVisual();
            }
        }

        /// <summary>
        /// Activates this connector, switching to the active sprite.
        /// </summary>
        public void Activate()
        {
            if (_currentState == ConnectorState.Activated) return;

            _currentState = ConnectorState.Activated;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (_image == null) return;
            _image.sprite = _currentState == ConnectorState.Activated ? activeSprite : dormantSprite;
        }
    }
}
