using UnityEngine;

namespace PixelPlatformer
{
    public class LoopingMovingPlatform : MovingPlatformBase
    {
        [SerializeField] private float _endWaitTime = 0f;

        private bool _movingToEnd = true;
        private float _waitTimer;

        private void FixedUpdate()
        {
            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.fixedDeltaTime;
                SetMoving(false);
                return;
            }

            Vector2 target = _movingToEnd ? _endPoint : _startPoint;

            if (StepTowards(target, _speed))
            {
                _movingToEnd = !_movingToEnd;
                _waitTimer = _endWaitTime;
            }
        }
    }
}