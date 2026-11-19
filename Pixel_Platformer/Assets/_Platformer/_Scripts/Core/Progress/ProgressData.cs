using System;
using System.Collections.Generic;

namespace PixelPlatformer
{
    [Serializable]
    public class ProgressData
    {
        public List<LevelRecord> records;

        public ProgressData(int totalLevels)
        {
            records = new List<LevelRecord>(totalLevels);



            for (int i = 0; i < totalLevels; i++)
            {
                records.Add(new LevelRecord
                {
                    levelIndex = i + 1,
                    isUnlocked = i == 0,   // level 1 unlocked by default
                    StarsEarned = 0,
                    bestScore = 0f
                });
            }
        }
    }
}