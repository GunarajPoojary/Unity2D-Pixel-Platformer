using DG.Tweening;
using TMPro;
using UnityEngine;

namespace PixelPlatformer
{
    public class Toaster : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TMP_Text _label;

        [SerializeField] private float _holdTime = 2f;
        [SerializeField] private float _fadeTime = 0.25f;
        [SerializeField] private float _slideDistance = 60f;

        private Vector2 _shownPos;
        private Sequence _seq;

        private void Awake()
        {
            _shownPos = _rect.anchoredPosition;
            _group.alpha = 0f;
        }

        private void OnDestroy()
        {
            _seq?.Kill();
        }

        public void Show(string message)
        {
            _seq?.Kill();

            _label.text = message;
            _group.alpha = 0f;
            _rect.anchoredPosition = _shownPos - new Vector2(0f, _slideDistance);

            _seq = DOTween.Sequence()
                          .Append(_group.DOFade(1f, _fadeTime))
                          .Join(_rect.DOAnchorPos(_shownPos, _fadeTime).SetEase(Ease.OutBack))
                          .AppendInterval(_holdTime)
                          .Append(_group.DOFade(0f, _fadeTime))
                          .SetLink(gameObject);
        }
    }
}