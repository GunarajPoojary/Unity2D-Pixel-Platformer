using UnityEngine;
using UnityEngine.Tilemaps;

namespace PixelPlatformer
{
    [RequireComponent(typeof(TilemapRenderer), typeof(Tilemap))]
    public class AnimatedBackground : MonoBehaviour
    {
        [SerializeField] private float _scrollSpeed = 5f;
        [SerializeField] private Tilemap _map;
        [SerializeField] private TileBase _targetTile;
        [SerializeField] private bool _compressBounds = true;

        private TilemapRenderer _renderer;
        private float _scrollDownPos;
        private Transform _mainCam;

        private void Awake()
        {
            if (_compressBounds)
                _map.CompressBounds();

            _renderer = GetComponent<TilemapRenderer>();

            SwapTiles(_targetTile);
        }

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
            float camHorizontalPos = _mainCam.position.x;
            Vector2 pos = transform.position;

            _scrollDownPos -= _scrollSpeed * Time.deltaTime;
            _scrollDownPos = Mathf.Repeat(_scrollDownPos, _renderer.bounds.extents.y / 2f);

            int targetHorizontalPos = (int)camHorizontalPos / ((int)transform.localScale.x * (int)_map.layoutGrid.cellSize.x);

            pos.x = targetHorizontalPos * transform.localScale.x * _map.layoutGrid.cellSize.x;
            pos.y = _scrollDownPos;

            transform.position = pos;
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