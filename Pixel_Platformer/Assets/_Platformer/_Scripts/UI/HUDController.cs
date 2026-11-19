using TMPro;
using UnityEngine;

namespace PixelPlatformer
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _enemiesDefeatedText;
        [SerializeField] private TMP_Text _fruitsCollectedCountText;
        [SerializeField] private TMP_Text _timerText;

        private float _elapsedTime;
        private bool _isTimeTicking;

        public float ElapsedTime { get { return _elapsedTime; } }

        public void SetCollectibles(int count)
        {
            _fruitsCollectedCountText.text = count.ToString();
        }

        public void SetEnemiesDefeated(int count)
        {
            _enemiesDefeatedText.text = count.ToString();
        }

        private void Update()
        {
            if (!_isTimeTicking) return;

            _elapsedTime += Time.unscaledDeltaTime; // use unscaled timeScale, so pausing doesn't freeze it
            SetTime(_elapsedTime);
        }

        public void SetTime(float seconds)
        {
            int mins = (int)(seconds / 60f);
            float secs = seconds - mins * 60f;
            _timerText.text = $"{mins:00}:{secs:00.00}";
        }

        public void StopTimer()
        {
            _isTimeTicking = false;
        }

        public void StartTimer()
        {
            _isTimeTicking = true;
            _elapsedTime = 0f;
        }
    }
}