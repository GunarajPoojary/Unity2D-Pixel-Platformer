using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class ConfirmDialog : MonoBehaviour
    {
        [SerializeField] private TMP_Text _messageLabel;
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;
        [SerializeField] private PanelScaleTween _panelScaleTween;

        public event Action OnCancelQuit;
        public event Action OnConfirmQuit;

        private void OnEnable()
        {
            _yesButton.onClick.AddListener(Confirm);
            _noButton.onClick.AddListener(Cancel);
        }

        private void OnDisable()
        {
            _yesButton.onClick.RemoveListener(Confirm);
            _noButton.onClick.RemoveListener(Cancel);
        }

        public void PlayOpen(string message, Action onComplete = null)
        {
            _messageLabel.text = message;

            _panelScaleTween.PlayOpen(() =>
            {
                onComplete?.Invoke();
            });
        }

        public void PlayClose( Action onComplete = null)
        {
            _panelScaleTween.PlayClose(() =>
            {
                onComplete?.Invoke();
            });
        }

        private void Confirm()
        {
            _panelScaleTween.PlayClose();
            OnConfirmQuit?.Invoke();
        }
        
        private void Cancel()
        {
            _panelScaleTween.PlayClose();
            OnCancelQuit?.Invoke();
        }
    }
}