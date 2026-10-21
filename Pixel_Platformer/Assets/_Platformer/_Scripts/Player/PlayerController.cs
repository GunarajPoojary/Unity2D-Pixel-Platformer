using System;
using System.Collections;
using UnityEngine;

namespace PixelPlatformer
{
    [RequireComponent(typeof(PlayerInput), typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IHittable, IFollowTargetProvider
    {
        [Header("SFX Clips")]
        [SerializeField] private AudioClip _jumpClip;
        [SerializeField] private AudioClip _landClip;
        [SerializeField] private AudioClip _hurtClip;

        [SerializeField] private PlayerRenderer _renderer;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerInput _input;
        [SerializeField] private Stomper _stomper;

        [SerializeField] private float _bodyCenter = 0.786f; // got it from boxcollider
        [SerializeField] private Transform _cameraFollowTarget;

        private bool _canFollowTarget = true;
        private Rigidbody2D _rb;
        private Collider2D _collider;
        private bool _isHittable = true;

        public Transform CameraFollowTarget { get { return _cameraFollowTarget; } }
        public int LookDirection { get { return _movement.LookDirection; } }
        public bool CanFollowTarget { get { return _canFollowTarget; } }

        public bool MovementEnabled { get; set; }

        public bool IsHittable { get { return _isHittable; } }

        public event Action OnFirstMove;

        #region Unity API Methods
        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            _movement.OnMove += HandleMove;
            _movement.OnTurn += HandleTurn;
            _movement.OnFall += HandleFall;
            _movement.OnLand += HandleLand;
            _movement.OnJump += HandleJump;
            _movement.OnWallSlide += HandleWallSlide;

            _stomper.OnStomp += HandleStomp;

            _input.OnJumpPerformed += HandleJumpPerformed;
            _input.OnJumpCanceled += HandleJumpCanceled;
        }

        private void OnDisable()
        {
            _movement.OnMove -= HandleMove;
            _movement.OnTurn -= HandleTurn;
            _movement.OnFall -= HandleFall;
            _movement.OnLand -= HandleLand;
            _movement.OnJump -= HandleJump;
            _movement.OnWallSlide -= HandleWallSlide;

            _stomper.OnStomp -= HandleStomp;

            _input.OnJumpPerformed -= HandleJumpPerformed;
            _input.OnJumpCanceled -= HandleJumpCanceled;
        }

        private void Update()
        {
            // if (_input.InputEnabled)
            _renderer.Move(_movement.IsRunning);

            if (!_movement.IsFlying) return;

            // below 0 on Y means the object has fallen off the bottom of the screen
            if (_movement.LinearVelocity.y < 0f && IsOutOfView())
            {
                _movement.RestoreControlledBody();
                GameEvents.Publish(new PlayerDiedEventData());

                gameObject.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            if (_movement.IsFlying) return;

            if (MovementEnabled)
            {
                _movement.SimulateMovement(_input.MoveInput);
                _stomper.HandleStomp();
            }

            _movement.ApplyMovement();
        }

        private bool IsOutOfView()
        {
            // viewport co-ordinate origin is (0,0) which is bottom left corner and top right corner is (1,1)
            // convert world position to viewport co-ordinate and then check if that value is less than 0 which is viewport bottom bound
            return Camera.main.WorldToViewportPoint(transform.position).y < 0f;
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
        public void HandleStomp()
        {
            _movement.PerformJump();
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
            _stomper.enabled = true;

            ResetHit();
            _isHittable = true;
            _renderer.ResetHit();

            _canFollowTarget = true;
        }

        public void ResetHit()
        {
            _movement.RestoreControlledBody();
            _collider.enabled = true;
            _renderer.SetDefaultOrder();
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

        public void TakeHit(Vector2 hitSourcePosition)
        {
            _isHittable = false;
            MovementEnabled = false;
            GameEvents.Publish(new CameraShakeEventData());

            _collider.enabled = false;
            _renderer.SetSortingOrder(50);

            int hitDirection = transform.position.x - hitSourcePosition.x > 0f ? 1 : -1;

            _movement.ApplyHitImpact(hitDirection);

            _canFollowTarget = false;
            _stomper.enabled = false;
            _renderer.TriggerHit();

            AudioManager.Instance.PlaySFX(_hurtClip);
        }
    }
}