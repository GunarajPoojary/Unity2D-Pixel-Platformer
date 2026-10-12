using System;
using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(RectTransform))]
    public class PopupIn : MonoBehaviour
    {
        [SerializeField] private Ease _tweenEase = Ease.OutBack;
        [SerializeField] private float _tweenDuration = 0.1f;

        private Sequence _sequence;
        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        public void PlayInTween(Action onComplete = null)
        {
            _sequence?.Kill();
            _rect.localScale = Vector3.zero;

            _sequence = DOTween.Sequence().Append(_rect.DOScale(Vector3.one, _tweenDuration).SetEase(_tweenEase)).SetLink(gameObject).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
    }
}
