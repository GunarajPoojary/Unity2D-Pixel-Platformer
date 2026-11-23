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

        public static float TotalScore(LevelResult result, LevelScoreDataSO levelScore, float estimatedCompletionTime)
        {
            float timeBonus = (estimatedCompletionTime * levelScore.timeBonus) + (estimatedCompletionTime - result.timeTaken) * levelScore.timePoint;

            return (result.fruitsCollected * levelScore.fruitPoint)
                         + (result.enemiesDefeated * levelScore.enemyPoint)
                         + timeBonus;
        }

        public static float MaxScore(LevelObjectives objectives, LevelScoreDataSO levelScore)
        {
            return (objectives.totalFruits * levelScore.fruitPoint)
                   + (objectives.totalEnemies * levelScore.enemyPoint)
                   + (objectives.estimatedCompletionTime * levelScore.timeBonus);
        }
    }
}