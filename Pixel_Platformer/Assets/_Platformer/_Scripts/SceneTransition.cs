using System;
using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    [Serializable]
    public struct Column
    {
        public Transform[] transforms;
    }

    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private GameObject _spritePrefab;
        [SerializeField] private Column[] columns;

        [SerializeField] private float _size = 1f;
        [SerializeField] private float _horizontalSpace = 2f;
        [SerializeField] private float _verticalSpace = 2f;
        [SerializeField] private int _columnCount = 20;
        [SerializeField] private int _rowCount = 12;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _columnDelay = 0.1f;

        private void Awake()
        {
            CreateGrid();
        }

        private void CreateGrid()
        {
            columns = new Column[_columnCount];

            float totalWidth = (_columnCount - 1) * _horizontalSpace;
            float totalHeight = (_rowCount - 1) * _verticalSpace;

            for (int x = 0; x < _columnCount; x++)
            {
                columns[x].transforms = new Transform[_rowCount];

                for (int y = 0; y < _rowCount; y++)
                {
                    GameObject bubble = Instantiate(_spritePrefab, transform);

                    bool even = x % 2 == 0;
                    float stagger = even ? 0f : _verticalSpace / 2f;
                    float posX = x * _horizontalSpace - totalWidth / 2f;
                    float posY = y * _verticalSpace - totalHeight / 2f + stagger;

                    bubble.transform.localPosition = new Vector3(posX, posY, 0f);
                    bubble.transform.localScale = Vector3.zero;
                    columns[x].transforms[y] = bubble.transform;
                }
            }
        }

        public void PlayTransitionClip()
        {
            for (int x = 0; x < columns.Length; x++)
            {
                float delay = x * _columnDelay;

                foreach (Transform bubble in columns[x].transforms)
                {
                    bubble.DOKill();
                    bubble.localScale = Vector3.zero;
                    bubble.DOScale(Vector3.one * _size, _duration).SetDelay(delay);
                }
            }
        }
    }
}