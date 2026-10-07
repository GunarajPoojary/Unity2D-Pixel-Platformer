using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PixelPlatformer
{
    public class GameProgressDataManager : Singleton<GameProgressDataManager>
    {
        private const string GAME_PROGRESS_KEY = "Game_Progress";

        private GameProgressData _progressData;
        private int _totalLevels;

        private void OnEnable()
        {
            GameEvents.Subscribe<UnlockLevelEvent>(UnlockLevel);
            GameEvents.Subscribe<CompletLevelEvent>(CompleteLevel);
        }

        private void OnDisable()
        {
            GameEvents.Unsubscribe<UnlockLevelEvent>(UnlockLevel);
            GameEvents.Unsubscribe<CompletLevelEvent>(CompleteLevel);
        }

        private void CompleteLevel(CompletLevelEvent data)
        {
            SetLevelRecordData(data.levelResult.levelIndex, data.levelResult.stars);
        }

        private void OnDestroy()
        {
            Save();
        }

        private void UnlockLevel(UnlockLevelEvent data)
        {
            UnlockLevel(data.levelIndex);
        }

        public async UniTask Init(int totalLevels)
        {
            // load saved progress data
            // if not found use default data with level 1
            // var data = new GameProgressData();
            // data.records.Add(new LevelRecord() { levelIndex = 2, hasCleared = false, starsEarned = 0 });
            // Debug.Log(JsonUtility.ToJson(data));
            _totalLevels = totalLevels;

            string json = PlayerPrefs.GetString(GAME_PROGRESS_KEY, string.Empty);
            // Debug.Log(loadedData);

            if (string.IsNullOrEmpty(json))
            {
                _progressData = new GameProgressData(_totalLevels);
                _progressData.records[0].isUnlocked = true;
                return;
            }

            _progressData = JsonUtility.FromJson<GameProgressData>(json);

            if (_progressData == null || _progressData.records == null || _progressData.records.Count == 0)
            {
                _progressData = new GameProgressData(_totalLevels);
                _progressData.records[0].isUnlocked = true;
            }
        }










        public List<LevelRecord> LoadData()
        {

            return _progressData.records;
        }

        public void Save()
        {
            PlayerPrefs.SetString(GAME_PROGRESS_KEY, JsonUtility.ToJson(_progressData));
            PlayerPrefs.Save();
        }

        public void SaveData(List<LevelRecord> records)
        {
            _progressData.records = records;

            PlayerPrefs.SetString(GAME_PROGRESS_KEY, JsonUtility.ToJson(_progressData));
            PlayerPrefs.Save();
        }

        public void SetLevelRecordData(int levelIndex, int stars)
        {
            LevelRecord record = _progressData.records[levelIndex - 1];

            // if recorded data found then override it by new best record
            record.isUnlocked = true;
            record.starsEarned = Mathf.Max(record.starsEarned, stars); // override with best result

            // also unlock the next level with new default record data
            UnlockLevel(levelIndex + 1);
        }

        public void UnlockLevel(int targetLevelIndex, int starsEarned = 0)
        {
            if (targetLevelIndex <= _totalLevels)
            {
                var record = _progressData.records[targetLevelIndex - 1];
                record.isUnlocked = true;
                record.starsEarned = starsEarned;
            }
        }

        public int GetLatestUnlockedLevel()
        {
            int latestLevel = 1;
            for (int i = 0; i < _progressData.records.Count; i++)
            {
                if (!_progressData.records[i].isUnlocked) return latestLevel;;

                latestLevel = _progressData.records[i].levelIndex;
            }

            return latestLevel;
        }

        public void ClearData()
        {
            PlayerPrefs.DeleteKey(GAME_PROGRESS_KEY);
        }
    }
}