using System;
using UnityEngine;

namespace PixelPlatformer
{
    [Serializable]
    public class LevelData
    {
        public int levelIndex;
        public Sprite levelIcon;
        public string levelName;
        public int totalFruits;
        public int totalEnemies;
        public float estimatedCompletionTime;
        public int maxStars = 3;
    }
}