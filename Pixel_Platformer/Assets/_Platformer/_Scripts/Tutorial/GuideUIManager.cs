using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    public class GuideUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _guideRoot;
        [SerializeField] private Ease _showTweenEase = Ease.OutCubic;
        [SerializeField] private float _showTweenDuration = 0.5f;

        private RectTransform _guideRootRect;
        private Vector2 _originalAnchoredPosition;

        private void Awake()
        {
            _guideRootRect = _guideRoot.GetComponent<RectTransform>();
            _originalAnchoredPosition = _guideRootRect.anchoredPosition;
            _guideRoot.SetActive(false);
        }

        public void ShowGuide(Vector3 anchoredPos)
        {
            _guideRootRect.anchoredPosition = anchoredPos;
            _guideRootRect.localScale = Vector3.zero;
            _guideRoot.SetActive(true);

            _guideRootRect.DOAnchorPos(_originalAnchoredPosition, _showTweenDuration).SetEase(_showTweenEase);
            _guideRootRect.DOScale(Vector3.one, _showTweenDuration).SetEase(_showTweenEase);
        }
    }
}