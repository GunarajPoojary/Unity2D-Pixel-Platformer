using TMPro;
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
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _enemiesDefeatedText;

        [SerializeField] private TMP_Text _collectibleCountText;

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

        public void SetCollectibles(int count)
        {
            _collectibleCountText.text = count.ToString();
        }
        
        public void SetTime(float seconds)
        {
            int mins = (int)(seconds / 60f);
            float secs = seconds - mins * 60f;
            _timerText.text = $"{mins:00}:{secs:00.00}";
        }

        public void SetEnemiesDefeated(int count)
        {
            _enemiesDefeatedText.text = count.ToString();
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
            GameManager.Instance.PauseGame();
        }
    }
}