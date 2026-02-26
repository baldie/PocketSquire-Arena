using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PocketSquire.Arena.Core;
using DG.Tweening;

namespace PocketSquire.Unity.UI
{
    /// <summary>
    /// Controls a single skill-tree node button.
    /// Manages visual state (Dormant/Available/Activated) via grayscale material
    /// and gates interactability on prerequisite nodes.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class NodeController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum NodeState { Dormant, Available, Activated }

        [Header("References")]
        public AudioSource audioSource;
        public AudioClip hoverSound;
        public AudioClip clickSound;

        [Header("Materials")]
        [Tooltip("Material applied when node is Dormant (grayscale shader)")]
        public Material grayscaleMaterial;

        [Header("States")]
        public NodeState initialState = NodeState.Dormant;

        [Header("Connections")]
        public List<NodeConnector> outgoingConnectors = new List<NodeConnector>();
        public List<NodeController> prerequisites = new List<NodeController>();

        [Header("Skill Node Data")]
        public PlayerClass.ClassName nodeClass;

        [Header("Hover Glow Settings")]
        public Color hoverGlowColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow/gold
        public Vector2 hoverGlowDistance = new Vector2(3f, -3f);

        [Header("Active Class Indicator Settings")]
        public float pulseScale = 1.15f;
        public float pulseDuration = 0.5f;

        private Image _image;
        private Button _button;
        private NodeState _currentState = NodeState.Dormant;
        private ClassTreeController _treeController;
        private Outline _hoverOutline;
        private Tween _pulseTween;
        private bool _isActiveClass = false;

        public NodeState CurrentState => _currentState;

        private void Awake()
        {
            if (!TryGetComponent( out _image)) Debug.LogError($"{name}: Missing Image component!");
            if (!TryGetComponent(out _button)) Debug.LogError($"{name}: Missing Button component!");

            _treeController = GetComponentInParent<ClassTreeController>();

            if (audioSource == null)
            {
                var audioObj = GameObject.Find("UIAudio");
                if (audioObj != null)
                {
                    audioSource = audioObj.GetComponent<AudioSource>();
                }
            }

            _hoverOutline = gameObject.AddComponent<Outline>();
            _hoverOutline.effectColor = hoverGlowColor;
            _hoverOutline.effectDistance = hoverGlowDistance;
            _hoverOutline.enabled = false;
        }

        private void Start()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnNodeClicked);
            }

            bool isUnlocked = initialState == NodeState.Activated ||
                              (GameState.Player != null && GameState.Player.UnlockedClasses.Contains(nodeClass.ToString()));

            if (isUnlocked)
            {
                Activate(false);
            }
            else
            {
                ApplyVisual();
                UpdateInteractable();
            }

            CheckPulseIndicator();
        }

        private void OnEnable()
        {
            if (_pulseTween == null)
            {
                CheckPulseIndicator();
            }
        }

        private void OnDisable()
        {
            if (_pulseTween != null)
            {
                _pulseTween.Kill();
                _pulseTween = null;
                transform.localScale = Vector3.one;
            }
        }

        public void CheckPulseIndicator()
        {
            // Only pulse if it is the currently selected class of the player
            if (GameState.Player != null)
            {
                if (GameState.Player.Class == nodeClass)
                {
                    _isActiveClass = true;

                    if (_pulseTween == null)
                    {
                        Debug.Log($"[NodeController] Starting pulse for {nodeClass} on {gameObject.name}");
                        _pulseTween = transform.DOScale(pulseScale, pulseDuration)
                            .SetUpdate(true)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
                    }

                    if (_hoverOutline != null)
                    {
                        _hoverOutline.effectColor = Color.cyan;
                        _hoverOutline.effectDistance = new Vector2(6f, -6f);
                        _hoverOutline.enabled = true;
                    }
                }
                else
                {
                    _isActiveClass = false;

                    if (_pulseTween != null)
                    {
                        Debug.Log($"[NodeController] Stopping pulse for {nodeClass} on {gameObject.name}");
                        _pulseTween.Kill();
                        _pulseTween = null;
                        transform.localScale = Vector3.one;
                    }

                    // Reset outline state in case it was cyan
                    if (_hoverOutline != null)
                    {
                        _hoverOutline.effectColor = hoverGlowColor;
                        _hoverOutline.effectDistance = hoverGlowDistance;
                        _hoverOutline.enabled = false;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[NodeController] GameState.Player is null when checking pulse on {gameObject.name}");
            }
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
#if UNITY_EDITOR
            if (_currentState == NodeState.Available || _currentState == NodeState.Activated)
#else
            if (_currentState == NodeState.Available)
#endif
            {
                if (audioSource != null && clickSound != null)
                {
                    audioSource.PlayOneShot(clickSound);
                }
                Activate(true);
            }
        }

        /// <summary>
        /// Activates this node: sets state to Activated, then
        /// activates all outgoing connectors.
        /// </summary>
        public void Activate(bool changePlayerClass = true)
        {
            SetState(NodeState.Activated);

            if (GameState.Player != null)
            {
                GameState.Player.UnlockedClasses.Add(nodeClass.ToString());

                if (changePlayerClass && GameState.Player.Class != nodeClass)
                {
                    GameState.Player.ChangeClass(nodeClass);
                }

                // Refresh all nodes' pulse indicators
                var allNodes = FindObjectsOfType<NodeController>();
                foreach (var node in allNodes)
                {
                    node.CheckPulseIndicator();
                }
            }

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

            // We may have just become the active class, check to start pulsing
            CheckPulseIndicator();
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

            // If already activated, still allow interactions to swap classes
            if (_currentState == NodeState.Activated)
            {
#if UNITY_EDITOR
                _button.interactable = true;
#else
                _button.interactable = false;
#endif
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

            // Ensure alpha is never < 1 (255)
            var color = _image.color;
            color.a = 1f;
            _image.color = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isActiveClass && (_currentState == NodeState.Available || _currentState == NodeState.Activated))
            {
                if (_hoverOutline != null)
                {
                    _hoverOutline.effectColor = hoverGlowColor;
                    _hoverOutline.effectDistance = hoverGlowDistance;
                    _hoverOutline.enabled = true;
                }
            }

            if (audioSource != null && hoverSound != null)
            {
                audioSource.PlayOneShot(hoverSound);
            }

            if (_treeController != null)
            {
                string desc = PlayerClass.GetDescription(nodeClass);
                _treeController.ShowHoverDescription(desc);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isActiveClass)
            {
                if (_hoverOutline != null) _hoverOutline.enabled = false;
            }

            if (_treeController != null)
            {
                _treeController.HideHoverDescription();
            }
        }
    }
}
