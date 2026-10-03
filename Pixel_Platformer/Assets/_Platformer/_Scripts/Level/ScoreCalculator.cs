using System;
using UnityEngine;

namespace PixelPlatformer
{
    public static class ScoreCalculator
    {
        public static int GetStars(float score, float maxScore, int maxStars)
        {
            float ratio = Mathf.Clamp01(score / maxScore);

            if (ratio >= 0.85f) return maxStars;
            if (ratio >= 0.5f) return Mathf.Max(1, maxStars - 1);

            return 1;
        }

        public static int Score(LevelResultData result, LevelScoreDataSO scoreData)
        {
            float timeBonus = Mathf.Max(0f, result.estimatedCompletionTime - result.timeTaken) * scoreData.timePoint;

            return Mathf.RoundToInt(timeBonus + (result.estimatedCompletionTime * scoreData.timePoint)
                                    + Mathf.Min(result.fruitsCollected, result.totalFruits)
                                    * scoreData.fruitPoint
                                    + Mathf.Min(result.enemiesDefeated, result.totalEnemies)
                                    * scoreData.enemyDefeatPoint);
        }

        public static float MaxScore(LevelData data, LevelScoreDataSO scoreData)
        {
            return data.estimatedCompletionTime
                * scoreData.timePoint
                + data.totalFruits
                * scoreData.fruitPoint
                + data.totalEnemies
                * scoreData.enemyDefeatPoint;
        }
    }
}