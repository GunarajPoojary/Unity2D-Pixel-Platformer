using UnityEngine;

namespace PixelPlatformer
{
    public class RunState : IState
    {
        private MushroomController _controller;
        private RunData _runData;

        public RunState(MushroomController controller)
        {
            _controller = controller;
            _runData = _controller.RunData;
        }

        public void Enter()
        {
            _controller.Animator.SetBool("isRun", true);
            _controller.RunFX.Play();
        }

        public void Exit()
        {
            _controller.Animator.SetBool("isRun", false);
            _controller.RunFX.Stop();
        }

        public void PhysicsUpdate() { }

        public void UpdateState()
        {
            Debug.Log($"Updating {GetType().Name}");
        }
    }
}