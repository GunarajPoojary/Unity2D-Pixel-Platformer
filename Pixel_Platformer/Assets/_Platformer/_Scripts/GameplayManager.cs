using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PixelPlatformer
{
    public class GameplayManager : Singleton<GameplayManager>
    {
        [SerializeField] private TutorialPanel _tutorialPanel;
        [SerializeField] private PlayerManager _playerManager;

        public async UniTask Initialize()
        {
            _tutorialPanel.Init();

            _playerManager.ToggleInput(false);
        }
    }
}
