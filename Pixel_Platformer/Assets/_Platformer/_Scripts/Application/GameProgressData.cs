using System;
using System.Collections.Generic;

namespace PixelPlatformer
{
    [Serializable]
    public class GameProgressData
    {
        public List<LevelRecord> records;

        public GameProgressData(int totalLevels)
        {
            records = new List<LevelRecord>();

            for (int i = 1; i <= totalLevels; i++)
            {
                records.Add(new LevelRecord(i));
            }
        }
    }
}