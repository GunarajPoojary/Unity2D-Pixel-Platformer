using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelPlatformer
{
    [RequireComponent(typeof(Slider))]
    public class AudioSliderPreview : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Toggle _muteToggle;
        [SerializeField] private AudioClip _previewClip;

        private bool _isDragging;

        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(HandleVolumeChanged);
            _muteToggle.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(HandleVolumeChanged);
            _muteToggle.onValueChanged.AddListener(HandleValueChanged);
        }

        private void HandleValueChanged(bool toggle)
        {
            if (!toggle)
            {
                PlayPreview();
            }
        }

        public void HandleVolumeChanged(float value)
        {
            // Ignore value changes caused by mouse/touch dragging.
            if (_isDragging)
            {
                return;
            }

            // Value changed through keyboard/gamepad navigation.
            if (EventSystem.current.currentSelectedGameObject == _slider.gameObject)
            {
                PlayPreview();
            }
        }

        private void PlayPreview()
        {
            AudioManager.Instance.PlaySFX(_previewClip);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;

            // Play once when the user finishes dragging.
            PlayPreview();
        }
    }
}