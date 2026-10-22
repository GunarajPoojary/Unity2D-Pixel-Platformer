using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PixelPlatformer
{
    public class GameplayManager : Singleton<GameplayManager>
    {
        [SerializeField] private TutorialPanel _tutorialPanel;
        [SerializeField] private PlayerManager _playerManager;

        private bool _inputWasEnabled;

        private void OnEnable()
        {
            _tutorialPanel.OnOpened += HandleTutorialOpened;
            _tutorialPanel.OnClosed += HandleTutorialClosed;
        }

        private void OnDisable()
        {
            _tutorialPanel.OnOpened -= HandleTutorialOpened;
            _tutorialPanel.OnClosed -= HandleTutorialClosed;
        }

        public async UniTask Initialize()
        {
            _tutorialPanel.Init();

            _playerManager.ToggleInput(false);
        }

        private void HandleTutorialOpened()
        {
            _inputWasEnabled = _playerManager.InputEnabled;
            _playerManager.ToggleInput(false);
        }

        private void HandleTutorialClosed()
        {
            if (_inputWasEnabled)
                _playerManager.ToggleInput(true);
        }
    }
}