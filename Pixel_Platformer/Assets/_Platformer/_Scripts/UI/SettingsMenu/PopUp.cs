using System;
using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(RectTransform))]
    public class PopUp : MonoBehaviour
    {
        [SerializeField] private float _hideDuration = 0.4f;
        [SerializeField] private float _popDuration = 0.5f;
        private Sequence _popupSequence;
        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

#if UNITY_EDITOR
        public void Show()
        {
            Open();
        }

        public void Hide()
        {
            Close();
        }
#endif

        public void Open(Action onComplete = null)
        {
            _popupSequence?.Kill();
            _rect.localScale = Vector3.zero;

            _popupSequence = DOTween.Sequence()
                .Append(_rect.DOScale(Vector3.one, _popDuration).SetEase(Ease.OutBack))
                .SetLink(gameObject).OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }

        public void Close(Action onComplete = null)
        {
            _popupSequence?.Kill();

            _popupSequence = DOTween.Sequence()
                .Append(_rect.DOScale(Vector3.zero, _hideDuration).SetEase(Ease.InBack))
                .SetLink(gameObject).OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }
    }
}
