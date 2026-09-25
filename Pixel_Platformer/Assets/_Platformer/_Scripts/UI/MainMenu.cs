using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button _playButton;

        private void OnEnable()
        {
            _playButton.onClick.AddListener(StartGame);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(StartGame);
        }

        private void StartGame()
        {
            GameManager.Instance.StartGame();
        }
    }
}
