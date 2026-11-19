using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelPlatformer
{
    public class ProgressionManager : Singleton<ProgressionManager>
    {
        private const string GAME_PROGRESS_KEY = "Game_Progress";

        private ProgressData _progressData;
        private int _totalLevels;

        public event Action OnDataErased;

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveData();
        }

        private void OnApplicationQuit()
        {
            SaveData();
        }

        private void OnDestroy()
        {
            SaveData();
        }

        public void Init(int totalLevels)
        {
            _totalLevels = totalLevels;

            string json = PlayerPrefs.GetString(GAME_PROGRESS_KEY, string.Empty);

            if (!string.IsNullOrEmpty(json))
                _progressData = JsonUtility.FromJson<ProgressData>(json);

            if (_progressData == null || _progressData.records == null || _progressData.records.Count == 0)
                _progressData = new ProgressData(totalLevels);
        }

        public List<LevelRecord> LoadData()
        {
            return _progressData.records;
        }

        public void SaveData()
        {
            PlayerPrefs.SetString(GAME_PROGRESS_KEY, JsonUtility.ToJson(_progressData));
            PlayerPrefs.Save();
        }

        public void SetLevelRecordData(int levelIndex, LevelResult result)
        {
            var index = levelIndex - 1;
            LevelRecord record = _progressData.records[index];

            // if recorded data found then override it by new best record
            record.isUnlocked = true;
            record.StarsEarned = Mathf.Max(record.StarsEarned, result.starsEarned);
            record.bestScore = Mathf.Max(record.bestScore, result.score);

            // also unlock the next level with new default record data
            var nextLevelIndex = levelIndex + 1;
            UnlockLevel(nextLevelIndex);
            SaveData();
        }

        public void UnlockLevel(int targetLevelIndex)
        {
            if (targetLevelIndex > _progressData.records.Count) return;

            var index = targetLevelIndex - 1;
            var record = _progressData.records[index];
            if (record.isUnlocked) return;

            record.isUnlocked = true;
            record.StarsEarned = 0;
            record.bestScore = 0;
        }

        public void EraseData()
        {
            PlayerPrefs.DeleteKey(GAME_PROGRESS_KEY);
            _progressData = new ProgressData(_totalLevels);
            SaveData();
            OnDataErased?.Invoke();
        }

        public LevelRecord GetRecord(int currentLevelIndex)
        {
            foreach (var item in _progressData.records)
            {
                if (item.levelIndex == currentLevelIndex)
                    return item;
            }

            return null;
        }
    }
}