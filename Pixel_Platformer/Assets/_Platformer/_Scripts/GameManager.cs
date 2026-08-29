using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class GameManager : MonoBehaviour
    {
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
