using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Button _guideButton;
        [SerializeField] private Button _replayLevelButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TutorialUI _guideUIManager;
        private Vector3 _guideButtonAnchorPos;

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

        public void Init()
        {
            _guideButtonAnchorPos = _guideButton.GetComponent<RectTransform>().anchoredPosition;
        }

        private void OpenGuide()
        {
            _guideUIManager.Toggle();
        }

        private void ReplayLevel()
        {
            GameManager.Instance.RestartLevel();
        }

        private void OpenSettings()
        {
            GameManager.Instance.OpenSettingsMenu();
        }
    }
}