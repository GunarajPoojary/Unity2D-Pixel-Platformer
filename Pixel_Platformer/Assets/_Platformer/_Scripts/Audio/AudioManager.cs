using System;
using UnityEngine;
using UnityEngine.Audio;

namespace PixelPlatformer
{
    public class AudioManager : Singleton<AudioManager>
    {
        private const string MASTER_PARAM = "MasterVolume";
        private const string MUSIC_PARAM = "MusicVolume";
        private const string SFX_PARAM = "SFXVolume";
        private const float AMPLITUDE_THRESHOLD = 0.0001f;
        private const float MUTE_VOLUME_DB = -80f;

        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioMixer _mixer;


        private float _masterVolume = 1f;
        private float _musicVolume = 1f;
        private float _sFXVolume = 1f;

        private bool _isMasterMuted;
        private bool _isMusicMuted;
        private bool _isSFXMuted;

        #region Master
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);

            if (!_isMasterMuted)
            {
                SetMixerVolume(MASTER_PARAM, _masterVolume);
            }
        }

        public void MuteMaster(bool toggle)
        {
            _isMasterMuted = toggle;

            if (toggle)
            {
                SetMixerMute(MASTER_PARAM);
            }
            else
            {
                SetMixerVolume(MASTER_PARAM, _masterVolume);
            }
        }
        #endregion

        #region Music
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

            if (!_isMusicMuted)
            {
                SetMixerVolume(MUSIC_PARAM, _musicVolume);
            }
        }

        public void MuteMusic(bool toggle)
        {
            _isMusicMuted = toggle;

            if (toggle)
            {
                SetMixerMute(MUSIC_PARAM);
            }
            else
            {
                SetMixerVolume(MUSIC_PARAM, _musicVolume);
            }
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

            if (!_isSFXMuted)
            {
                SetMixerVolume(SFX_PARAM, _sFXVolume);
            }
        }

        public void MuteSFX(bool toggle)
        {
            _isSFXMuted = toggle;

            if (toggle)
            {
                SetMixerMute(SFX_PARAM);
            }
            else
            {
                SetMixerVolume(SFX_PARAM, _sFXVolume);
            }
        }
        #endregion


        private void SetMixerVolume(string name, float amplitude)
        {
            // _mixer.SetFloat(name, amplitude < AMPLITUDE_THRESHOLD ? ToDecibel(AMPLITUDE_THRESHOLD) : ToDecibel(amplitude));
            _mixer.SetFloat(name, ToDecibel(Mathf.Clamp(amplitude, AMPLITUDE_THRESHOLD, 1)));
        }

        private void SetMixerMute(string parameterName)
        {
            _mixer.SetFloat(parameterName, MUTE_VOLUME_DB);
        }

        /// <summary>
        /// Converts a linear amplitude value to decibels (dB).
        /// Uses the formula: dB = 20 × log10(amplitude).
        /// </summary>
        /// <param name="amplitude">
        /// The linear amplitude value. Must be at least 0.0001.
        /// </param>
        /// <returns>
        /// The amplitude expressed in decibels (dB).
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="amplitude"/> is less than 0.0001.
        /// </exception>
        public float ToDecibel(float amplitude)
        {
            if (amplitude < AMPLITUDE_THRESHOLD)
                throw new ArgumentOutOfRangeException(
                nameof(amplitude),
                amplitude,
                "Amplitude must be at least 0.0001.");

            // Formula: dB = 20 × log10(amplitude)
            return Mathf.Log10(amplitude) * 20f;
        }
    }
}
