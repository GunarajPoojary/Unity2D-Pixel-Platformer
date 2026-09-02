using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class GameManager : Singleton<GameManager>
    {
        private void Start()
        {
            PlayerManager.Instance.Init();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            GameEvents.Clear();
        }
    }
}
