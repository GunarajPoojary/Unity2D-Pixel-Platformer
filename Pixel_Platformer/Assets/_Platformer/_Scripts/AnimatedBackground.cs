using UnityEngine;
using UnityEngine.Tilemaps;

namespace PixelPlatformer
{
    [RequireComponent(typeof(TilemapRenderer), typeof(Tilemap))]
    public class AnimatedBackground : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private Tilemap _map;
        [SerializeField] private TileBase _targetTile;
        [SerializeField] private bool _compressBounds = true;

        private TilemapRenderer _renderer;
        private Vector3 _targetPos;
        private Transform _mainCam;

        private void Awake()
        {
            if (_compressBounds)
                _map.CompressBounds();

            _renderer = GetComponent<TilemapRenderer>();

            SwapTiles(_targetTile);
        }

        // [ContextMenu("Extent")]
        // private void PrintExtents()
        // {
        //     Debug.Log(_map.layoutGrid.cellSize);
        // }

        private void Start()
        {
            _mainCam = Camera.main.transform;
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            Vector2 camPos = _mainCam.position;
            Vector2 bgPos = transform.position;

            _targetPos += _speed * Time.deltaTime * Vector3.down;
            _targetPos.y = Mathf.Repeat(_targetPos.y, _renderer.bounds.extents.y * 0.5f);

            Vector2Int desiredBGPos = new Vector2Int((int)camPos.x / ((int)transform.localScale.x * (int)_map.layoutGrid.cellSize.x), (int)camPos.y / ((int)transform.localScale.y * (int)_map.layoutGrid.cellSize.y));
            // int targetXPos = (int)camPos.x / ((int)transform.localScale.x * (int)_map.layoutGrid.cellSize.x);
            // int targetYPos = (int)camPos.y / ((int)transform.localScale.y * (int)_map.layoutGrid.cellSize.y);

            bgPos.x = desiredBGPos.x * transform.localScale.x * _map.layoutGrid.cellSize.x;
            bgPos.y = _targetPos.y + (desiredBGPos.y * transform.localScale.y * _map.layoutGrid.cellSize.y);

            transform.position = bgPos;
        }

        public void SwapTiles(TileBase newtile)
        {
            foreach (Vector3Int position in _map.cellBounds.allPositionsWithin)
            {
                if (_map.HasTile(position))
                {
                    _map.SetTile(position, newtile);
                }
            }
        }
    }
}