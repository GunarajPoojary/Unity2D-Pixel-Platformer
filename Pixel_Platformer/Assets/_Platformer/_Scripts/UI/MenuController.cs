using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private Button _guideButton;
        [SerializeField] private Button _replayLevelButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TutorialPanel _tutorialPanel;

        private void OnEnable()
        {
            _guideButton.onClick.AddListener(OpenGuide);
            _replayLevelButton.onClick.AddListener(ReplayLevel);
            _settingsButton.onClick.AddListener(OpenSettings);
        }

        private void OnDisable()
        {
            _guideButton.onClick.RemoveListener(OpenGuide);
            _replayLevelButton.onClick.RemoveListener(ReplayLevel);
            _settingsButton.onClick.RemoveListener(OpenSettings);
        }

        private void OpenGuide()
        {
            _tutorialPanel.Toggle();
        }

        private void ReplayLevel()
        {
            GameManager.Instance.RestartLevel();
        }

        private void OpenSettings()
        {
            GameplayManager.Instance.PauseGame();
        }
    }
}