using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private GameObject _mobileInputUIRoot;

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
        [SerializeField] private MobileInputButton _leftButton;
        [SerializeField] private MobileInputButton _rightButton;
        [SerializeField] private MobileInputButton _jumpButton;
        [SerializeField] private bool _useMobileInput;
#endif

        private bool _inputEnabled = true;
        private float _moveInput;

        public float MoveInput { get { return _moveInput; } }

        public bool InputEnabled { get { return _inputEnabled; } set { _inputEnabled = value; } }

        public event Action OnJumpPerformed;
        public event Action OnJumpCanceled;

        public void Init()
        {
#if UNITY_EDITOR
            _mobileInputUIRoot.SetActive(_useMobileInput);
#elif UNITY_ANDROID || UNITY_IOS
    _mobileInputUIRoot.SetActive(true);
#else
    _mobileInputUIRoot.SetActive(false);
#endif
        }

        private void Update()
        {
            if (!InputEnabled) return;

#if UNITY_EDITOR
            if (_useMobileInput)
                ReadMobileInput();
            else
                ReadKeyboardInput();
#elif UNITY_ANDROID || UNITY_IOS
                ReadMobileInput();
#else
            ReadKeyboardInput();
#endif
        }

        private void ReadMobileInput()
        {
            if (_leftButton.Pressed)
            {
                _moveInput = -1;
            }
            else if (_rightButton.Pressed)
            {
                _moveInput = 1;
            }
            else
            {
                _moveInput = 0;
            }

            if (_jumpButton.PressedThisFrame)
            {
                OnJumpPerformed?.Invoke();
            }

            if (_jumpButton.ReleasedThisFrame)
            {
                OnJumpCanceled?.Invoke();
            }
        }

        private void ReadKeyboardInput()
        {
            _moveInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump"))
            {
                OnJumpPerformed?.Invoke();
            }

            if (Input.GetButtonUp("Jump"))
            {
                OnJumpCanceled?.Invoke();
            }
        }
    }
}