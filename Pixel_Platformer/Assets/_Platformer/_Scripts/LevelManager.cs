using UnityEngine;

namespace PixelPlatformer
{
    public class LevelManager : Singleton<LevelManager>
    {
        [SerializeField] private StartCheckpoint _startCheckpoint;
        [SerializeField] private float _spawnYOffset = 1f;
        [SerializeField] private SceneReference[] _levels;

        private int _currentLevelIndex = 0;

        [ContextMenu("Start Level")]
        public void StartLevel()
        {

        }
    }
}