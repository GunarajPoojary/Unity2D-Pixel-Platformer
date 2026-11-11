using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace PixelPlatformer
{
    public class PathLine : MonoBehaviour
    {
        [SerializeField] private Transform[] _points;
        [SerializeField] private bool _closed;

        [SerializeField] private Transform _pathRoot;
        [SerializeField] private GameObject _chainPrefab;
        [SerializeField] private float _chainSpacing = 0.5f;

        private float[] _cumulative;
        private float _length;

        public bool IsClosed
        {
            get { return _closed; }
        }

        public float Length
        {
            get { return _length; }
        }

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            _length = 0f;

            if (_points == null || _points.Length < 2)
            {
                _cumulative = new float[0];
                return;
            }

            int segments = SegmentCount();
            _cumulative = new float[segments];

            for (int i = 0; i < segments; i++)
            {
                _length += Vector2.Distance(PointAt(i), PointAt(i + 1));
                _cumulative[i] = _length;
            }
        }

        public Vector2 GetPosition(float distance)
        {
            if (_length <= 0f)
                return _points != null && _points.Length > 0 ? (Vector2)_points[0].position : (Vector2)transform.position;

            distance = _closed ? Mathf.Repeat(distance, _length) : Mathf.Clamp(distance, 0f, _length);

            float segmentStart = 0f;

            for (int i = 0; i < _cumulative.Length; i++)
            {
                if (distance <= _cumulative[i])
                {
                    float segmentLength = _cumulative[i] - segmentStart;
                    float t = segmentLength > 0f ? (distance - segmentStart) / segmentLength : 0f;

                    return Vector2.Lerp(PointAt(i), PointAt(i + 1), t);
                }

                segmentStart = _cumulative[i];
            }

            return PointAt(_points.Length - 1);
        }

        private int SegmentCount()
        {
            return _closed ? _points.Length : _points.Length - 1;
        }

        private Vector2 PointAt(int index)
        {
            return _points[index % _points.Length].position;
        }

        private void OnDrawGizmos()
        {
            if (_points == null || _points.Length == 0) return;

            Gizmos.color = Color.green;

            for (int i = 0; i < _points.Length; i++)
            {
                if (_points[i] == null) continue;

                Gizmos.DrawWireSphere(_points[i].position, 0.15f);

                bool hasNext = i < _points.Length - 1 || (_closed && _points.Length > 2);

                if (!hasNext) continue;

                Transform next = _points[(i + 1) % _points.Length];

                if (next != null)
                    Gizmos.DrawLine(_points[i].position, next.position);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _chainSpacing = Mathf.Max(0.1f, _chainSpacing);

            if (Application.isPlaying || EditorUtility.IsPersistent(this)) return;

            EditorApplication.delayCall -= RebuildChain;
            EditorApplication.delayCall += RebuildChain;
        }

        [ContextMenu("Rebuild Chain")]
        private void RebuildChain()
        {
            if (this == null || _pathRoot == null || _chainPrefab == null) return;
            if (_points == null || _points.Length < 2) return;

            for (int i = 0; i < _points.Length; i++)
            {
                if (_points[i] == null) return;
            }

            Build();

            for (int i = _pathRoot.childCount - 1; i >= 0; i--)
                DestroyImmediate(_pathRoot.GetChild(i).gameObject);

            if (_length <= 0f) return;

            int segments = Mathf.Max(1, Mathf.RoundToInt(_length / _chainSpacing));
            float step = _length / segments;
            int count = _closed ? segments : segments + 1;

            for (int i = 0; i < count; i++)
            {
                GameObject link = PrefabUtility.InstantiatePrefab(_chainPrefab, _pathRoot) as GameObject;
                if (link == null) break;

                Vector2 position = GetPosition(i * step);
                link.transform.position = new Vector3(position.x, position.y, _pathRoot.position.z);
            }

            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}