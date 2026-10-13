using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Button), typeof(RectTransform))]
    public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ISubmitHandler
    {
        [SerializeField] private ButtonFeedbackConfigSO _config;

        private RectTransform _rect;
        private Button _button;
        private Vector3 _originalScale;
        private Tween _tween;
        private bool _isPressed;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rect = GetComponent<RectTransform>();

            if (_rect == null)
                _rect = (RectTransform)transform;

            _originalScale = _rect.localScale;
        }

        private void OnDisable()
        {
            _tween?.Kill();
            _rect.localScale = _originalScale;
            _isPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.IsInteractable()) return;

            _isPressed = true;
            PlayClick();
            ScaleTo(_originalScale * _config.PressedScale, _config.PressDuration, _config.PressEase);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Release();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!_button.IsInteractable()) return;
            PlayClick();
        }

        private void Release()
        {
            if (!_isPressed) return;

            _isPressed = false;
            ScaleTo(_originalScale, _config.ReleaseDuration, _config.ReleaseEase);
        }

        private void ScaleTo(Vector3 scale, float duration, Ease ease)
        {
            _tween?.Kill();
            _tween = _rect.DOScale(scale, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void PlayClick()
        {
            if (_config.ClickClip != null)
                AudioManager.Instance.PlaySFX(_config.ClickClip);
        }
    }
}