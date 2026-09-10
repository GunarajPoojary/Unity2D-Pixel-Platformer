using System;
using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    [Serializable]
    public class SingleClipAnimator
    {
        public MonoBehaviour monoBehaviour;
        public SpriteRenderer _renderer;
        public Sprite[] _sprites;
        public bool _loop;
        public int _frameRate = 12;
        private int _keyFrame;
        private Coroutine _coroutine;

        public event Action OnEndPlay;

        public void PlayClip()
        {
            if (_coroutine != null)
                StopClip();

            _coroutine = monoBehaviour.StartCoroutine(PlayClipRoutine());
        }

        public void StopClip()
        {
            monoBehaviour.StopCoroutine(_coroutine);
            _coroutine = null;
        }

        private IEnumerator PlayClipRoutine()
        {
            _keyFrame = 0;
            var elapsedTime = 0f;
            var targetDeltaTime = 1f / _frameRate;

            // for every 1/12the time i.e 12fps considering default framerate, jump to next keyframe
            while (_keyFrame < _sprites.Length)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= targetDeltaTime)
                {
                    _renderer.sprite = _sprites[_keyFrame];
                    _keyFrame++;
                    elapsedTime -= targetDeltaTime;
                }

                if (_loop)
                    _keyFrame %= _sprites.Length;

                yield return null;
            }

            OnEndPlay?.Invoke();
        }
    }
}