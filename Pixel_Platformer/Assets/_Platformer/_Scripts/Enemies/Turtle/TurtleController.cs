using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Hittable))]
    public class TurtleController : MonoBehaviour
    {
        [SerializeField] private float _idleNoSpikeTime = 1f;
        [SerializeField] private float _idleSpikeTime = 1f;

        [SerializeField] private SingleClipAnimator _idleNoSpike;
        [SerializeField] private SingleClipAnimator _spikeOut;
        [SerializeField] private SingleClipAnimator _idleSpike;
        [SerializeField] private SingleClipAnimator _spikeIn;

        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private ContactHazard _hazard = new ContactHazard();
        [SerializeField] private StompImpactTrigger _stompTrigger;

        private Hittable _hittable;
        private SingleClipAnimator _currentClip;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _hittable = GetComponent<Hittable>();

            if (_stompTrigger == null)
                _stompTrigger = GetComponent<StompImpactTrigger>();

            SetupClip(_idleNoSpike, true);
            SetupClip(_idleSpike, true);
            SetupClip(_spikeOut, false);
            SetupClip(_spikeIn, false);
        }

        private void OnEnable()
        {
            _hittable.OnHit += HandleHit;
            StartCoroutine(SpikeCycle());
        }

        private void OnDisable()
        {
            _hittable.OnHit -= HandleHit;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            _hazard.TryHit(col, transform.position);
        }

        private void SetupClip(SingleClipAnimator clip, bool loop)
        {
            clip.monoBehaviour = this;
            clip._renderer = _renderer;
            clip._loop = loop;
        }

        private IEnumerator SpikeCycle()
        {
            while (true)
            {
                // Idle, spikes in
                SetSpikes(false);
                _hazard.SetIdleNoSpike();
                Play(_idleNoSpike);
                yield return new WaitForSeconds(_idleNoSpikeTime);

                // Spikes coming out (transition: no damage)
                SetSpikes(true);
                bool done = false;
                Play(_spikeOut, () => done = true);
                yield return new WaitUntil(() => done);

                // Idle, spikes out
                _hazard.SetIdleSpike();
                Play(_idleSpike);
                yield return new WaitForSeconds(_idleSpikeTime);

                // Spikes going in (transition: no damage)
                _hazard.Disable();
                done = false;
                Play(_spikeIn, () => done = true);
                yield return new WaitUntil(() => done);
            }
        }

        private void Play(SingleClipAnimator clip, System.Action onComplete = null)
        {
            if (_currentClip != null && _currentClip != clip)
                _currentClip.StopClip();

            _currentClip = clip;
            clip.PlayClip(onComplete);
        }

        private void SetSpikes(bool spikesOn)
        {
            _hittable.enabled = !spikesOn;
            _hazard.Disable();

            if (_stompTrigger != null)
                _stompTrigger.enabled = !spikesOn;
        }

        private void HandleHit()
        {
            StopAllCoroutines();

            _currentClip?.StopClip();

            _hazard.Disable();

            if (_stompTrigger != null)
                _stompTrigger.enabled = false;

            enabled = false;
        }
    }
}