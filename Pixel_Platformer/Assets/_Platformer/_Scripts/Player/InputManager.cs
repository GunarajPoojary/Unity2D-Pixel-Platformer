using UnityEngine;

namespace PixelPlatformer
{
    public class InputManager : Singleton<InputManager>
    {
        [SerializeField] private PlayerInput _playerInput;

        protected override void Awake()
        {
            base.Awake();
            
            _playerInput.Init();
            TogglePlayerInput(false);
        }

        public void TogglePlayerInput(bool toggle)
        {
            _playerInput.InputEnabled = toggle;
        }
    }
}