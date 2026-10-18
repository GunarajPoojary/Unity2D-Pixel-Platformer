using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace PixelPlatformer
{
    public class TutorialPanel : MonoBehaviour
    {
        [Serializable]
        private class ClipLabelPair
        {
            public VideoClip clip;
            public string label;
        }

        [SerializeField] private ClipLabelPair[] _clipLabelPairs;
        [SerializeField] private VideoPlayer _videoPlayer;

        [SerializeField] private PanelScaleTween _panelScaleTween;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _guideLabel;

        private int _currentClipIndex;
        private bool _isOpen;

        public void Init()
        {
            _isOpen = false;
            SetClip(0);
            _videoPlayer.Stop();
        }

        private void OnEnable()
        {
            _previousButton.onClick.AddListener(PlayPreviousClip);
            _nextButton.onClick.AddListener(PlayNextClip);
            _closeButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            _previousButton.onClick.RemoveListener(PlayPreviousClip);
            _nextButton.onClick.RemoveListener(PlayNextClip);
            _closeButton.onClick.RemoveListener(Hide);
        }

        public void Show()
        {
            if (_isOpen) return;
            _isOpen = true;

            _videoPlayer.Play();
            _panelScaleTween.PlayOpen();
        }

        public void Hide()
        {
            if (!_isOpen) return;
            _isOpen = false;

            _panelScaleTween.PlayClose(() =>
            {
                _videoPlayer.Stop();
            });
        }

        public void Toggle()
        {
            if (_isOpen)
                Hide();
            else
                Show();
        }

        private void SetClip(int index)
        {
            _currentClipIndex = index;
            _videoPlayer.clip = _clipLabelPairs[index].clip;

            _guideLabel.text = _clipLabelPairs[index].label;

            _previousButton.gameObject.SetActive(index > 0);
            _nextButton.gameObject.SetActive(index < _clipLabelPairs.Length - 1);
        }

        private void PlayClip(int index)
        {
            SetClip(index);
            _videoPlayer.Play();
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