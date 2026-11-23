using UnityEngine;

namespace PixelPlatformer
{
    [System.Serializable]
    public class LevelRecord
    {
        public int levelIndex;
        public int StarsEarned;
        public bool isUnlocked;
        public float bestScore;
    }




    [System.Serializable]
    public struct LevelResult
    {
        public int starsEarned;
        public int fruitsCollected;
        public int enemiesDefeated;
        public int totalFruits;
        public int totalEnemies;
        public float timeTaken;
        public float score;
        public float bestScore;
        public bool isNewBest;
    }
    [CreateAssetMenu(menuName = "Level Data", fileName = "Data/Level")]
    public class LevelDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public int LevelIndex { get; private set; }
        [field: SerializeField] public Sprite LevelIcon { get; private set; }
        [field: SerializeField] public SceneReference LevelScene { get; private set; }
    }
}