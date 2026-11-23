using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace PixelPlatformer
{
    public enum PlatformAxis
    {
        Horizontal,
        Vertical
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class MovingPlatformBase : MonoBehaviour
    {
        [Header("Path")]
        [SerializeField] protected PlatformAxis _axis = PlatformAxis.Horizontal;
        [SerializeField] protected float _distance = 4f;
        [SerializeField] protected float _speed = 2f;

        [Header("Animation")]
        [SerializeField] private SingleClipAnimator _moveClip;

        [Header("Chain Path")]
        [SerializeField] private Transform _pathRoot;
        [SerializeField] private GameObject _chainPrefab;
        [SerializeField] private float _chainSpacing = 0.5f;

        [Header("Rider")]
        [SerializeField] protected LayerMask _targetMask;
        [SerializeField, Range(0.1f, 1f)] private float _topContactThreshold = 0.5f;

        protected Rigidbody2D _rb;
        protected Vector2 _startPoint;
        protected Vector2 _endPoint;
        protected Vector2 _currentPosition;

        private Rigidbody2D _rider;
        private bool _isMoving;

        protected bool HasRider
        {
            get { return _rider != null; }
        }

        protected Vector2 EndOffset
        {
            get { return _axis == PlatformAxis.Horizontal ? new Vector2(_distance, 0f) : new Vector2(0f, _distance); }
        }

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            _startPoint = _rb.position;
            _endPoint = _startPoint + EndOffset;
            _currentPosition = _startPoint;

            _moveClip.monoBehaviour = this;
            if (_moveClip._renderer == null)
                _moveClip._renderer = GetComponentInChildren<SpriteRenderer>();

            if (_pathRoot != null && _pathRoot.IsChildOf(transform))
                _pathRoot.SetParent(transform.parent, true);
        }

        protected virtual void OnDisable()
        {
            _rider = null;
            _isMoving = false;
            _moveClip.StopClip();
        }

        protected void SetMoving(bool moving)
        {
            if (_isMoving == moving) return;

            _isMoving = moving;

            if (moving)
            {
                _moveClip.PlayClip();
                return;
            }

            _moveClip.StopClip();

            if (_moveClip._sprites != null && _moveClip._sprites.Length > 0)
                _moveClip._renderer.sprite = _moveClip._sprites[0];
        }

        protected bool StepTowards(Vector2 target, float speed)
        {
            Vector2 next = Vector2.MoveTowards(_currentPosition, target, speed * Time.fixedDeltaTime);
            Vector2 delta = next - _currentPosition;

            _currentPosition = next;
            _rb.position = next;

            SetMoving(delta != Vector2.zero);

            if (_rider != null)
                _rider.position += delta;

            return next == target;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            EvaluateContact(col);
        }

        private void OnCollisionStay2D(Collision2D col)
        {
            EvaluateContact(col);
        }

        private void OnCollisionExit2D(Collision2D col)
        {
            if (col.rigidbody == _rider)
                _rider = null;
        }

        private void EvaluateContact(Collision2D col)
        {
            if ((_targetMask.value & (1 << col.gameObject.layer)) == 0) return;
            if (col.rigidbody == null) return;

            _rider = IsOnTop(col) ? col.rigidbody : null;
        }

        private bool IsOnTop(Collision2D col)
        {
            for (int i = 0; i < col.contactCount; i++)
            {
                if (-col.GetContact(i).normal.y > _topContactThreshold)
                    return true;
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 start = Application.isPlaying ? _startPoint : (Vector2)transform.position;
            Vector2 end = Application.isPlaying ? _endPoint : start + EndOffset;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(start, 0.15f);
            Gizmos.DrawWireSphere(end, 0.15f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _chainSpacing = Mathf.Max(0.1f, _chainSpacing);

            if (Application.isPlaying || EditorUtility.IsPersistent(this)) return;

            EditorApplication.delayCall -= RebuildChain;
            EditorApplication.delayCall += RebuildChain;
        }

        private void RebuildChain()
        {
            if (this == null || _pathRoot == null || _chainPrefab == null) return;

            for (int i = _pathRoot.childCount - 1; i >= 0; i--)
                DestroyImmediate(_pathRoot.GetChild(i).gameObject);

            float distance = EndOffset.magnitude;
            int segments = Mathf.Max(1, Mathf.RoundToInt(distance / _chainSpacing));
            int count = distance < 0.01f ? 1 : segments + 1;
            Vector3 start = transform.position;

            for (int i = 0; i < count; i++)
            {
                GameObject link = PrefabUtility.InstantiatePrefab(_chainPrefab, _pathRoot) as GameObject;
                if (link == null) break;

                float t = segments > 0 ? (float)i / segments : 0f;
                link.transform.position = start + (Vector3)EndOffset * t;
            }

            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}