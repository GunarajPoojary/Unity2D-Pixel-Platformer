using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PixelPlatformer
{
    public class AudioManager : Singleton<AudioManager>
    {
        private const string MUSIC_KEY = "MusicVolume";
        private const string SFX_KEY = "SFXVolume";

        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

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
