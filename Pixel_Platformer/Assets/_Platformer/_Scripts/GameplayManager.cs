using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PixelPlatformer
{
    public class GameplayManager : Singleton<GameplayManager>
    {
        [SerializeField] private HUD _hUD;
        [SerializeField] private TutorialUI _tutorialUI;
        [SerializeField] private PlayerManager _playerManager;

        public async UniTask Initialize()
        {
            _hUD.Init();
            _tutorialUI.Init();

            _playerManager.ToggleInput(false);
        }
    }
}
