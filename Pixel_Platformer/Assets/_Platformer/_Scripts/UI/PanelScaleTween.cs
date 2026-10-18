using System;
using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    public class PanelScaleTween : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _contentCanvasgroup;
        [SerializeField] private PanelScaleTweenConfigSO _config;
        [SerializeField] private RectTransform _rect;

        private void Start()
        {
            _contentCanvasgroup.alpha = 0f;
            _contentCanvasgroup.interactable = false;
            _contentCanvasgroup.blocksRaycasts = false;
        }

        public void PlayOpen(Action onComplete = null)
        {
            _contentCanvasgroup.alpha = 1f;
            _contentCanvasgroup.interactable = false;
            _contentCanvasgroup.blocksRaycasts = true;

            _rect.localScale = Vector3.one * _config.StartScale;

            _rect.DOKill();
            _rect.DOScale(1f, _config.OpenDuration)
                 .SetEase(_config.OpenEase)
                 .SetLink(_rect.gameObject)
                 .OnComplete(() =>
                 {
                     _contentCanvasgroup.interactable = true;
                     onComplete?.Invoke();
                 });
        }

        public void PlayClose(Action onComplete = null)
        {
            _contentCanvasgroup.interactable = false;

            _rect.DOKill();
            _rect.DOScale(_config.StartScale, _config.CloseDuration)
                 .SetEase(_config.CloseEase)
                 .SetLink(_rect.gameObject)
                 .OnComplete(() =>
                 {
                     _contentCanvasgroup.blocksRaycasts = false;
                     _contentCanvasgroup.alpha = 0f;

                     onComplete?.Invoke();
                 });
        }

#if UNITY_EDITOR
        [ContextMenu("Test Open")]
        private void TestOpen()
        {
            PlayOpen(() =>
            {
                Debug.Log("Open complete", this);
            });
        }

        [ContextMenu("Test Close")]
        private void TestClose()
        {
            PlayClose(() =>
            {
                Debug.Log("Close complete", this);
            });
        }
#endif
    }
}