using System;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider _sFXVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Button _eraseButton;
        [SerializeField] private PanelScaleTween _panelScaleTween;
        [SerializeField] private Button _closeButton;

        public event Action EraseRequested;
        public event Action OnCloseClicked;

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        public void PlayOpen(Action onComplete = null)
        {
            _sFXVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            _musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            _eraseButton.onClick.AddListener(EraseProgress);

            _panelScaleTween.PlayOpen(() =>
            {
                onComplete?.Invoke();
            });

            SetSliders();
        }

        public void PlayClose(Action onComplete = null)
        {
            _sFXVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
            _musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
            _eraseButton.onClick.RemoveListener(EraseProgress);

            AudioManager.Instance.SaveData();

            _panelScaleTween.PlayClose(() =>
            {
                onComplete?.Invoke();
            });
        }

        private void HandleCloseClicked()
        {
            PlayClose();
            OnCloseClicked?.Invoke();
        }

        private void SetSFXVolume(float value)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        private void SetMusicVolume(float value)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        private void EraseProgress()
        {
            EraseRequested?.Invoke();
        }

        private void SetSliders()
        {
            _sFXVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolume);
            _musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
        }
    }
}
