using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioSource _sfxAudioSource;

        public void PlayOneShotAudio(AudioClip clip)
        {
            _sfxAudioSource.PlayOneShot(clip);
        }
    }
}
