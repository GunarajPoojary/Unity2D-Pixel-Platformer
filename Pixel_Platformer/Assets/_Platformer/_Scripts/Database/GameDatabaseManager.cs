using UnityEngine;

namespace PixelPlatformer
{
    public class GameDatabaseManager : MonoBehaviour
    {
        [SerializeField] private LevelDefinitionSO[] _levels;

        public int TotalLevels { get { return _levels.Length; } }

        public Sprite GetLevelIcon(int levelIndex)
        {
            int index = levelIndex - 1;

            if (index < 0 || index >= TotalLevels)
            {
                Debug.LogError($"Cannot get icon. Invalid level index: {levelIndex}");
                return null;
            }

            return _levels[index].LevelIcon;
        }
    }
}