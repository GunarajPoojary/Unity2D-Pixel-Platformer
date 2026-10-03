using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class LoseScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Image _skullIcon;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _subtitleText;

        [Header("Stats")]
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _bestText;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;


        private Sequence _sequence;

        private void Start()
        {
            Hide();
        }

        private void OnEnable()
        {
            _retryButton.onClick.AddListener(Retry);
            _mainMenuButton.onClick.AddListener(MainMenu);
        }

        private void OnDisable()
        {
            _retryButton.onClick.RemoveListener(Retry);
            _mainMenuButton.onClick.RemoveListener(MainMenu);
            _sequence?.Kill();
        }

        public void Show(LevelResultData result)
        {
           Debug.Log("Show Lose screen");
        }

        public void Hide()
        {
            _sequence?.Kill();
            _root.SetActive(false);
        }

        private void Retry()
        {
            Hide();
            GameManager.Instance.RestartLevel();
        }

        private void MainMenu()
        {
            Hide();
            GameManager.Instance.GoToMainMenu();
        }
    }
}