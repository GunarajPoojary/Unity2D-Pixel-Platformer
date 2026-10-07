using System;

namespace PixelPlatformer
{
    [Serializable]
    public class LevelRecord
    {
        public int levelIndex;
        public bool isUnlocked;
        public int starsEarned;

        public LevelRecord(int levelIndex)
        {
            this.levelIndex = levelIndex;
            isUnlocked = false;
            starsEarned = 0;
        }
    }
}