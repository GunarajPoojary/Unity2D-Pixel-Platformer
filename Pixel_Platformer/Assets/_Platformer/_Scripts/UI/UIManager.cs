using UnityEngine;

namespace PixelPlatformer
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private TutorialPanel _tutorialPanel;
        [SerializeField] private HUD _hUD;
        [SerializeField] private PauseMenu _pauseMenu;

        [SerializeField] private WinScreen _winScreen;
        [SerializeField] private LoseScreen _loseScreen;

        public void ShowWinScreen(LevelResult result)
        {
            _winScreen.Open(result);
        }

        public void HideWinScreen()
        {
            _winScreen.Close();
        }

        public void SetTimer(float seconds)
        {
            _hUD.SetTime(seconds);
        }

        public void SetupWinScreen(Sprite levelIcon, int totalEnemies, int totalFruits)
        {
            _winScreen.Setup(levelIcon, totalEnemies, totalFruits);
        }

        public void SetupGameplayUI()
        {
            _tutorialPanel.Init();
        }

        public void ShowPauseMenu()
        {
            _pauseMenu.Open();
        }

        public void HidePauseMenu()
        {
            _pauseMenu.Close();
        }

        public void SetCollectibles(int count)
        {
            _hUD.SetCollectibles(count);
        }

        public void ShowLoseScreen()
        {
            _loseScreen.Open();
        }

        public void HideLoseScreen()
        {
            _loseScreen.Close();
        }

        public void SetEnemiesDefeated(int count)
        {
            _hUD.SetEnemiesDefeated(count);
        }
    }
}