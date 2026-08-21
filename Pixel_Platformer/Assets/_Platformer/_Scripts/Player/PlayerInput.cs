using System;
using UnityEngine;

namespace PixelPlatform
{
    public class PlayerInput : MonoBehaviour
    {
        private Vector2 _moveInput;

        public Vector2 MoveInput
        {
            get
            {
                return _moveInput;
            }
        }

        public float LookDirection
        {
            get
            {
                return Mathf.Sign(_moveInput.x);
            }
        }

        public event Action OnJumpPerformed;
        public event Action OnJumpCanceled;

        private void Update()
        {
            _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

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