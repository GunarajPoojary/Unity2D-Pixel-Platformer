using System;
using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(PlayerInput), typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IImpactable, IFollowTargetProvider
    {
        [Header("SFX Clips")]
        [SerializeField] private AudioClip _jumpClip;
        [SerializeField] private AudioClip _landClip;
        [SerializeField] private AudioClip _hurtClip;

        [SerializeField] private PlayerRenderer _renderer;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerInput _input;

        [SerializeField] private float _bodyCenter = 0.786f; // got it from boxcollider
        [SerializeField] private Transform _cameraFollowTarget;

        private bool _canFollowTarget = true;
        private Hittable _hittable;

        public Transform CameraFollowTarget { get { return _cameraFollowTarget; } }
        public int LookDirection { get { return _movement.LookDirection; } }
        public bool CanFollowTarget { get { return _canFollowTarget; } }

        public bool MovementEnabled { get; set; }

        public event Action OnFirstMove;

        #region Unity API Methods
        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _hittable = GetComponent<Hittable>();
        }

        private void OnEnable()
        {
            _movement.OnMove += HandleMove;
            _movement.OnTurn += HandleTurn;
            _movement.OnFall += HandleFall;
            _movement.OnLand += HandleLand;
            _movement.OnJump += HandleJump;
            _movement.OnWallSlide += HandleWallSlide;

            _input.OnJumpPerformed += HandleJumpPerformed;
            _input.OnJumpCanceled += HandleJumpCanceled;
            _hittable.OnHit += HandleHit;
        }

        private void OnDisable()
        {
            _movement.OnMove -= HandleMove;
            _movement.OnTurn -= HandleTurn;
            _movement.OnFall -= HandleFall;
            _movement.OnLand -= HandleLand;
            _movement.OnJump -= HandleJump;
            _movement.OnWallSlide -= HandleWallSlide;

            _input.OnJumpPerformed -= HandleJumpPerformed;
            _input.OnJumpCanceled -= HandleJumpCanceled;
            _hittable.OnHit -= HandleHit;
        }

        private void Update()
        {
            _renderer.Move(_movement.IsRunning);
        }

        private void FixedUpdate()
        {
            if (MovementEnabled)
                _movement.SimulateMovement(_input.MoveInput);

            _movement.ApplyMovement();
        }
        #endregion

        private void HandleMove()
        {
            OnFirstMove?.Invoke();
        }

        private void HandleTurn(bool turn)
        {
            _renderer.Turn(turn);
        }

        private void HandleFall()
        {
            _renderer.Fall();
        }

        private void HandleLand()
        {
            AudioManager.Instance.PlaySFX(_landClip);
            _renderer.Land();
        }

        private void HandleJump()
        {
            _renderer.Jump();
            AudioManager.Instance.PlaySFX(_jumpClip);
            OnFirstMove?.Invoke();
        }

        private void HandleWallSlide(bool slide)
        {
            _renderer.SlideWall(slide);
        }

        private void HandleJumpCanceled()
        {
            _movement.CutJump();
        }

        private void HandleJumpPerformed()
        {
            _movement.BufferJump();
        }

        #region Hit Methods
        public void ApplyImpact(float impact)
        {
            _movement.PerformJump(impact);
        }

        private void HandleHit()
        {
            _canFollowTarget = false;
            GetComponent<Stomper>().enabled = false;
            _input.enabled = false;
            _renderer.TriggerHit();

            enabled = false;
            AudioManager.Instance.PlaySFX(_hurtClip);
            GameEvents.Publish(new PlayerDiedEventData());
        }
        #endregion

        public void SetPosition(Vector3 pos)
        {
            pos.y -= _bodyCenter;
            transform.position = pos;
        }

        public void DespawnAt(Vector3 position, Action onComplete)
        {
            MovementEnabled = false;
            StartCoroutine(DespawnRoutine(position, onComplete));
            _renderer.Jump();
        }

        public void ResetState()
        {
            _movement.ResetState();
            enabled = true;
            if (TryGetComponent(out Stomper stomper)) stomper.enabled = true;

            _hittable.ResetHit();
            _renderer.ResetHit();

            _canFollowTarget = true;
        }

        private IEnumerator DespawnRoutine(Vector3 position, Action onComplete)
        {
            position.y -= _bodyCenter;
            while (!_movement.StepTowards(position))
                yield return null;

            gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        public void Spawn()
        {
            gameObject.SetActive(true);
            _renderer.Turn(true);
            _renderer.Fall();
            MovementEnabled = true;
            ResetState();
        }

        public void Despawn()
        {
            gameObject.SetActive(false);
        }

        public void ToggleInput(bool toggle)
        {
            _input.InputEnabled = toggle;
        }
    }
}