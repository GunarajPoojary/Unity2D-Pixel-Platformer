using UnityEngine;

namespace PixelPlatformer
{
    [CreateAssetMenu(menuName = "Score Data", fileName = "Data/Score")]
    public class LevelScoreDataSO : ScriptableObject
    {
        public int timePoint;
        public int fruitPoint;
        public int enemyDefeatPoint;
    }
}