using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using PocketSquire.Unity;

namespace PocketSquire.Unity.UI
{
    /// <summary>
    /// A reusable confirmation dialog that can be triggered from any script.
    /// Usage: ConfirmationDialog.Show(dialogInstance, "Are you sure?", OnConfirm, OnCancel);
    /// </summary>
    public class ConfirmationDialog : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform dialogPanel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private TextMeshProUGUI yesButtonText;
        [SerializeField] private TextMeshProUGUI noButtonText;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip confirmSound;
        [SerializeField] private AudioClip cancelSound;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;

        private Action _onConfirm;
        private Action _onCancel;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            InitializeCanvasGroup();
        }

        private void InitializeCanvasGroup()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            WireButtons();
        }

        private void WireButtons()
        {
            if (yesButton != null)
            {
                yesButton.onClick.RemoveAllListeners();
                yesButton.onClick.AddListener(OnYesClicked);
            }

            if (noButton != null)
            {
                noButton.onClick.RemoveAllListeners();
                noButton.onClick.AddListener(OnNoClicked);
            }
        }

        private void Update()
        {
            // Allow Cancel input to dismiss the dialog
            if (gameObject.activeSelf && InputManager.GetButtonDown("Cancel"))
            {
                InputManager.ConsumeButton("Cancel");
                OnNoClicked();
            }
        }

        /// <summary>
        /// Shows the confirmation dialog with a custom message and callbacks.
        /// </summary>
        /// <param name="dialog">The ConfirmationDialog instance in the scene.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="onConfirm">Callback invoked when the user clicks Yes.</param>
        /// <param name="onCancel">Optional callback invoked when the user clicks No or presses Cancel.</param>
        /// <param name="yesText">Optional custom text for the Yes button (default: "Yes").</param>
        /// <param name="noText">Optional custom text for the No button (default: "No").</param>
        public static void Show(
            ConfirmationDialog dialog,
            string message,
            Action onConfirm,
            Action onCancel = null,
            string yesText = "Yes",
            string noText = "No")
        {
            if (dialog == null)
            {
                Debug.LogError("[ConfirmationDialog] Dialog instance is null. Ensure a ConfirmationDialog exists in the scene.");
                return;
            }

            dialog.ShowInternal(message, onConfirm, onCancel, yesText, noText);
        }

        private void ShowInternal(string message, Action onConfirm, Action onCancel, string yesText, string noText)
        {
            InitializeCanvasGroup();

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            if (messageText != null)
            {
                messageText.text = message;
            }

            if (yesButtonText != null)
            {
                yesButtonText.text = yesText;
            }

            if (noButtonText != null)
            {
                noButtonText.text = noText;
            }

            // Activate and animate in
            gameObject.SetActive(true);
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, animationDuration).SetUpdate(true);
            }

            if (dialogPanel != null)
            {
                dialogPanel.gameObject.SetActive(true);
                dialogPanel.localScale = Vector3.one * 0.8f;
                dialogPanel.DOScale(Vector3.one, animationDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }

            // Select the No button by default (safer option)
            if (noButton != null)
            {
                EventSystem.current.SetSelectedGameObject(noButton.gameObject);
            }
        }

        private void OnYesClicked()
        {
            if (audioSource != null && confirmSound != null)
            {
                audioSource.PlayOneShot(confirmSound);
            }

            Hide(immediate: false, onComplete: () =>
            {
                _onConfirm?.Invoke();
                _onConfirm = null;
                _onCancel = null;
            });
        }

        private void OnNoClicked()
        {
            if (audioSource != null && cancelSound != null)
            {
                audioSource.PlayOneShot(cancelSound);
            }

            Hide(immediate: false, onComplete: () =>
            {
                _onCancel?.Invoke();
                _onConfirm = null;
                _onCancel = null;
            });
        }

        private void Hide(bool immediate, Action onComplete = null)
        {
            if (immediate)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                }
                gameObject.SetActive(false);
                onComplete?.Invoke();
                return;
            }

            Sequence hideSequence = DOTween.Sequence().SetUpdate(true);
            
            if (_canvasGroup != null)
            {
                hideSequence.Join(_canvasGroup.DOFade(0f, animationDuration).SetUpdate(true));
            }

            if (dialogPanel != null)
            {
                hideSequence.Join(dialogPanel.DOScale(Vector3.one * 0.8f, animationDuration)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true));
            }

            hideSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }
    }
}
