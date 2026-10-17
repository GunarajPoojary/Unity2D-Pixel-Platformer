using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    public class AudioManager : Singleton<AudioManager>
    {
        private const string MUSIC_KEY = "MusicVolume";
        private const string SFX_KEY = "SFXVolume";

        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        private Tween _musicFadeTween;

        private float _musicVolume = 1f;
        private float _sFXVolume = 1f;

        public float SFXVolume { get { return _sFXVolume; } }
        public float MusicVolume { get { return _musicVolume; } }

        private void OnDestroy()
        {
            SaveData();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveData();
        }

        private void OnApplicationQuit()
        {
            SaveData();
        }

        public async UniTask Initialize()
        {
            LoadData();
        }

        #region Music
        public void SetMusic(bool loop, AudioClip clip)
        {
            _musicSource.loop = loop;
            _musicSource.clip = clip;
        }

        public void PlayMusic()
        {
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        public void PauseMusic()
        {
            _musicSource.Pause();
        }

        public void ResumeMusic()
        {
            _musicSource.UnPause();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);

            _musicSource.volume = volume;
        }
        
        public async UniTask PlayMusicWithFade(AudioClip clip, float duration, bool loop = true)
        {
            // already playing this track, leave it alone
            if (_musicSource.isPlaying && _musicSource.clip == clip) return;

            KillMusicFade();

            _musicSource.Stop();
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.volume = 0f;
            _musicSource.Play();

            // fade up to the player's saved music volume, not a hardcoded 1
            Tween tween = DOTween.To(() => _musicSource.volume, v => _musicSource.volume = v, _musicVolume, duration)
                .SetUpdate(true)
                .SetLink(gameObject);
            _musicFadeTween = tween;

            await tween.AsyncWaitForCompletion();

            if (_musicFadeTween == tween) _musicFadeTween = null;
        }

        public async UniTask FadeOutMusic(float duration)
        {
            if (!_musicSource.isPlaying) return;

            KillMusicFade();

            Tween tween = DOTween.To(() => _musicSource.volume, v => _musicSource.volume = v, 0f, duration)
                .SetUpdate(true)
                .SetLink(gameObject);
            _musicFadeTween = tween;

            await tween.AsyncWaitForCompletion();

            // a newer fade took over while we were waiting, don't stop its music
            if (_musicFadeTween != tween) return;

            _musicSource.Stop();
            _musicFadeTween = null;
        }

        private void KillMusicFade()
        {
            _musicFadeTween?.Kill();
            _musicFadeTween = null;
        }
        #endregion


        #region SFX
        public void PlaySFX(AudioClip clip)
        {
            _sfxSource.PlayOneShot(clip);
        }

        public void SetSFXVolume(float volume)
        {
            _sFXVolume = Mathf.Clamp01(volume);

            _sfxSource.volume = volume;
        }
        #endregion

        #region Save/Load
        private void LoadData()
        {
            _musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
            _sFXVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);

            _musicSource.volume = _musicVolume;
            _sfxSource.volume = _sFXVolume;
        }

        public void SaveData()
        {
            PlayerPrefs.SetFloat(MUSIC_KEY, _musicVolume);
            PlayerPrefs.SetFloat(SFX_KEY, _sFXVolume);
            PlayerPrefs.Save();
        }
        #endregion
    }
}
