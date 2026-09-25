using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    public class TransitionSpritesColumn : MonoBehaviour
    {
        [SerializeField] private RectTransform[] _spriteRects;
        [SerializeField] private float _tweenDuration = 0.5f;
        [SerializeField] private Ease _tweenEase = Ease.InSine;

        private void Awake()
        {
            ResetState();
        }

        public void ResetState()
        {
            foreach (RectTransform rect in _spriteRects)
            {
                rect.localScale = Vector3.zero;
            }
        }

        [ContextMenu("Pop up")]
        public Sequence PopUp()
        {
            transform.DOKill();

            Sequence sequence = DOTween.Sequence();

            foreach (RectTransform rect in _spriteRects)
            {
                sequence.Join(rect.DOScale(Vector3.one, _tweenDuration).SetEase(_tweenEase));
            }

            return sequence;
        }
        
        [ContextMenu("Pop down")]
        public Sequence PopDown()
        {
            transform.DOKill();

            Sequence sequence = DOTween.Sequence();

            foreach (RectTransform rect in _spriteRects)
            {
                sequence.Join(rect.DOScale(Vector3.zero, _tweenDuration).SetEase(_tweenEase));
            }

            return sequence;
        }
    }
}