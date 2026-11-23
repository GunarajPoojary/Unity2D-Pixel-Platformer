using UnityEngine;

namespace PixelPlatformer
{
    public class TriggeredMovingPlatform : MovingPlatformBase
    {
        [SerializeField] private float _returnSpeed = 2f;

        private void FixedUpdate()
        {
            if (HasRider)
                StepTowards(_endPoint, _speed);
            else
                StepTowards(_startPoint, _returnSpeed);
        }
    }
}