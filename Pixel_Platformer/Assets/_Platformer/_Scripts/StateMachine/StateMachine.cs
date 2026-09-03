namespace PixelPlatformer
{
    public abstract class StateMachine
    {
        protected IState _currentState;

        public void SwitchState(IState newState)
        {
            _currentState?.Exit();

            _currentState = newState;

            _currentState.Enter();
        }

        public void UpdateState() => _currentState?.UpdateState();

        public void PhysicsUpdate() => _currentState?.PhysicsUpdate();
    }
}