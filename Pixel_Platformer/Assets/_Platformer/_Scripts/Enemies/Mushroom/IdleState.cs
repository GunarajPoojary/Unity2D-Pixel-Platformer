using UnityEngine;

namespace PixelPlatformer
{
    public class IdleState : IState
    {
        private MushroomController _controller;
        private IdleData _idleData;
        private float _idleTimer;

        public IdleState(MushroomController controller)
        {
            _controller = controller;
            _idleData = _controller.IdleData;
        }

        public void Enter()
        {
            _idleTimer = 0f;
        }

        public void Exit() { }

        public void PhysicsUpdate() { }

        public void UpdateState()
        {
            _idleTimer += Time.deltaTime;

            if (_idleTimer > _idleData.idleTime)
            {
                _controller.StateMachine.SwitchState(_controller.StateMachine.RunState);
            }
        }
    }
}