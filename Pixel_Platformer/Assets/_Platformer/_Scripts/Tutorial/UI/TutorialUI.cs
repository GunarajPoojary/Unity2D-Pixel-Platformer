using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Canvas))]
    public class TutorialUI : MonoBehaviour
    {
        [Serializable]
        private class ClipLabelPair
        {
            public VideoClip clip;
            public string label;
        }

        [SerializeField] private ClipLabelPair[] _clipLabelPairs;
        [SerializeField] private VideoPlayer _videoPlayer;

        [SerializeField] private CanvasGroup _root;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TMP_Text _guideLabel;

        [SerializeField] private Ease _showTweenEase = Ease.OutCubic;
        [SerializeField] private float _showTweenDuration = 0.5f;

        [SerializeField] private UnityEvent _onShow;
        [SerializeField] private UnityEvent _onHide;

        private Canvas _canvas;
        private int _currentClipIndex;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
            _videoPlayer.Pause();
        }

        private void OnEnable()
        {
            _previousButton.onClick.AddListener(PlayPreviousClip);
            _nextButton.onClick.AddListener(PlayNextClip);
        }

        private void OnDisable()
        {
            _previousButton.onClick.RemoveListener(PlayPreviousClip);
            _nextButton.onClick.RemoveListener(PlayNextClip);
        }

        private void Start()
        {
            PlayClip(0);
        }

        public void Show()
        {
            _root.alpha = 0f;
            _canvas.enabled = true;

            _root.DOFade(1f, _showTweenDuration).SetEase(_showTweenEase).OnComplete(OnShow);
        }

        public void Hide()
        {
            _root.DOFade(0f, _showTweenDuration).SetEase(_showTweenEase).OnComplete(OnHide);
        }

        private void OnShow()
        {
            _videoPlayer.Play();
            _onShow?.Invoke();
        }

        private void OnHide()
        {
            _canvas.enabled = false;
            _videoPlayer.Stop();
            _onHide?.Invoke();
        }

        public void Toggle()
        {
            if (_canvas.enabled)
                Hide();
            else
                Show();
        }

        private void PlayClip(int index)
        {
            _currentClipIndex = index;
            _videoPlayer.clip = _clipLabelPairs[index].clip;
            _videoPlayer.Play();

            _guideLabel.text = _clipLabelPairs[index].label;

            _previousButton.interactable = index > 0;
            _nextButton.interactable = index < _clipLabelPairs.Length - 1;
        }

        private void PlayNextClip()
        {
            PlayClip(_currentClipIndex + 1);
        }

        private void PlayPreviousClip()
        {
            PlayClip(_currentClipIndex - 1);
        }
    }
}