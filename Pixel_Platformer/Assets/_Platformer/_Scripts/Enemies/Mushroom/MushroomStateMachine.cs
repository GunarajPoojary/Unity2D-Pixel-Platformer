namespace PixelPlatformer
{
    public class MushroomStateMachine : StateMachine
    {
        public MushroomController Controller { get; }

        public RunState RunState { get; }
        public IdleState IdleState { get; }

        public MushroomStateMachine(MushroomController controller)
        {
            Controller = controller;
            RunState = new RunState(Controller);
            IdleState = new IdleState(controller);
        }
    }
}