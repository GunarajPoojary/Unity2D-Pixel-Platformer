using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private StartCheckpoint _startCheckpoint;

      //  [ContextMenu("Start Game")]
        private void Start()
        {
            StartCoroutine(StartGameRoutine());
            // wait untill he stands on the platform
            // once on the platform popup control guide UI
            // wait until the user presses input that shown on the control guide
            // once user presses right input then hide the control guide UI and continue the game            
        }

        private IEnumerator StartGameRoutine()
        {
            // first spawn the player
            PlayerManager.Instance.SpawnPlayer();

            yield return new WaitUntil(()=>_startCheckpoint.IsPlayerOn);

            InputManager.Instance.TogglePlayerInput(true);
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
