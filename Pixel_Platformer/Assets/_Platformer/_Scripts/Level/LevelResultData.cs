using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class LevelResultData
    {
        public int levelIndex;
        public string levelName;
        public Sprite levelIcon;
        public float timeTaken;
        public int fruitsCollected;
        public int totalFruits;
        public int totalEnemies;
        public int enemiesDefeated;
        public float estimatedCompletionTime;
        public float score;
        public float maxScore;
        public int stars;
    }
}