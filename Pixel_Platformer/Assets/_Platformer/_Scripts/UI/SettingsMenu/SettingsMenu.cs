using System;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Slider _sFXVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Button _eraseButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private PopUp _popup;

        private void OnEnable()
        {
            _sFXVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            _musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            _eraseButton.onClick.AddListener(EraseProgress);
        }

        private void OnDisable()
        {
            _sFXVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
            _musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
            _eraseButton.onClick.RemoveListener(EraseProgress);
        }

        private void Start()
        {
            _root.SetActive(false);
            _canvasGroup.interactable = false;
        }

        public void Open(Action onComplete = null)
        {
            _root.SetActive(true);

            _popup.Open(() =>
                {
                    _canvasGroup.interactable = true;
                    onComplete?.Invoke();
                });

            RefreshUI();
        }

        public void Close(Action onComplete = null)
        {
            _canvasGroup.interactable = false;
            AudioManager.Instance.SaveData();

            _popup.Close(() =>
                {
                    onComplete?.Invoke();
                    _root.SetActive(false);
                });
        }

        public void SetSFXVolume(float value)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        public void SetMusicVolume(float value)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        public void EraseProgress()
        {
            GameProgressDataManager.Instance.EraseData();
        }

        public void RefreshUI()
        {
            _sFXVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolume);
            _musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
        }
    }
}
