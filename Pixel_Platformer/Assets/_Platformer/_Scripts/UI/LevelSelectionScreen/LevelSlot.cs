using System;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class LevelSlot : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _levelIcon;

        [SerializeField] private Image[] _starImages;
        [SerializeField] private Sprite _lockSprite;
        [SerializeField] private Sprite _starOnSprite;
        [SerializeField] private Sprite _starOffSprite;
        private int _index;

        public bool IsSelectable
        {
            get
            {
                return _button.interactable;
            }
        }

        public event Action<int> Clicked;

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        public void BindData(int index, Sprite levelSprite, bool unlocked, int stars)
        {
            _index = index;

            _levelIcon.sprite = unlocked ? levelSprite : _lockSprite;
            _levelIcon.enabled = _levelIcon.sprite != null;

            _button.interactable = unlocked;

            if (unlocked)
            {
                for (int i = 0; i < _starImages.Length; i++)
                    _starImages[i].sprite = i < stars ? _starOnSprite : _starOffSprite;
            }
            else
            {
                for (int i = 0; i < _starImages.Length; i++)
                    _starImages[i].sprite = _starOffSprite;
            }
        }

        public void Select()
        {
            if (_button.interactable)
                _button.Select();
        }

        private void HandleClick()
        {
            Clicked?.Invoke(_index);
        }
    }
}
