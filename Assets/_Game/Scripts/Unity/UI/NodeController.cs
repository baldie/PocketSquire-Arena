using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PocketSquire.Unity.UI
{
    /// <summary>
    /// Controls a single skill-tree node button.
    /// Manages visual state (Dormant/Available/Activated) via grayscale material
    /// and gates interactability on prerequisite nodes.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class NodeController : MonoBehaviour
    {
        public enum NodeState { Dormant, Available, Activated }

        [Header("References")]
        public AudioSource audioSource;

        [Header("Materials")]
        [Tooltip("Material applied when node is Dormant (grayscale shader)")]
        public Material grayscaleMaterial;

        [Header("Connections")]
        public List<NodeConnector> outgoingConnectors = new List<NodeConnector>();
        public List<NodeController> prerequisites = new List<NodeController>();

        private Image _image;
        private Button _button;
        private NodeState _currentState = NodeState.Dormant;

        public NodeState CurrentState => _currentState;

        private void Awake()
        {
            if (!TryGetComponent( out _image)) Debug.LogError($"{name}: Missing Image component!");
            if (!TryGetComponent(out _button)) Debug.LogError($"{name}: Missing Button component!");
        }

        private void Start()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnNodeClicked);
            }
            ApplyVisual();
            UpdateInteractable();
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnNodeClicked);
            }
        }

        /// <summary>
        /// Handles button click. Activates the node if it's currently Available (and thus interactable).
        /// </summary>
        public void OnNodeClicked()
        {
            if (_currentState == NodeState.Available)
            {
                Activate();
            }
        }

        /// <summary>
        /// Activates this node: sets state to Activated, then
        /// activates all outgoing connectors.
        /// </summary>
        public void Activate()
        {
            SetState(NodeState.Activated);

            foreach (var connector in outgoingConnectors)
            {
                if (connector != null)
                {
                    connector.Activate();
                    // Notify the target node of this connector that a prerequisite (this node) is met
                    if (connector.targetNode != null)
                    {
                        connector.targetNode.UpdateInteractable();
                    }
                }
            }
        }

        /// <summary>
        /// Transition this node to a new state and refresh visuals.
        /// </summary>
        public void SetState(NodeState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            ApplyVisual();
            UpdateInteractable();
        }

        /// <summary>
        /// Button is interactable only when the node is Available
        /// (prerequisites empty or at least one prerequisite is Activated).
        /// Activated and Dormant nodes are not clickable.
        /// </summary>
        public void UpdateInteractable()
        {
            if (_button == null) return;

            // If already activated, not interactable (but visually distinct)
            if (_currentState == NodeState.Activated)
            {
                _button.interactable = false;
                return;
            }

            // Check if available
            bool isAvailable = prerequisites.Count == 0 || 
                               prerequisites.Any(p => p != null && p.CurrentState == NodeState.Activated);

            if (isAvailable && _currentState != NodeState.Available)
                SetState(NodeState.Available);
            else if (!isAvailable && _currentState != NodeState.Dormant)
                SetState(NodeState.Dormant);

            _button.interactable = _currentState == NodeState.Available;
        }

        private void ApplyVisual()
        {
            if (_image == null) return;

            // Dormant → grayscale shader; Available/Activated → default UI shader
            _image.material = _currentState == NodeState.Dormant ? grayscaleMaterial : null;
        }
    }
}
