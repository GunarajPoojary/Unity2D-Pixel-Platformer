using System;
using UnityEngine;

namespace PixelPlatformer
{
    [Flags]
    public enum Direction
    {
        None = 0,
        Up = 1 << 0,
        Down = 1 << 1,
        Right = 1 << 2,
        Left = 1 << 3,
    }

    [Serializable]
    public class ContactHazard
    {
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private Direction _idleSpikeDirections;
        [SerializeField] private Direction _idleNoSpikeDirections;

        private Direction _activeDirections;

        public Direction ActiveDirections
        {
            get
            {
                return _activeDirections;
            }
        }

        public void SetIdleSpike()
        {
            _activeDirections = _idleSpikeDirections;
        }

        public void SetIdleNoSpike()
        {
            _activeDirections = _idleNoSpikeDirections;
        }

        public void Disable()
        {
            _activeDirections = Direction.None;
        }

        public void TryHit(Collision2D col, Vector2 hitPosition)
        {
            if (_activeDirections == Direction.None) return;

            if ((_targetMask.value & (1 << col.gameObject.layer)) == 0) return;

            if (!col.collider.TryGetComponent<IHittable>(out var hittable) || !hittable.IsHittable)
                return;

            for (int i = 0; i < col.contactCount; i++)
            {
                Vector2 toTarget = -col.GetContact(i).normal;

                if ((_activeDirections & GetSide(toTarget)) != 0)
                {
                    hittable.TakeHit(hitPosition);
                    return;
                }
            }
        }

        private static Direction GetSide(Vector2 toTarget)
        {
            if (Mathf.Abs(toTarget.y) >= Mathf.Abs(toTarget.x))
                return toTarget.y > 0f ? Direction.Up : Direction.Down;

            return toTarget.x > 0f ? Direction.Right : Direction.Left;
        }
    }
}