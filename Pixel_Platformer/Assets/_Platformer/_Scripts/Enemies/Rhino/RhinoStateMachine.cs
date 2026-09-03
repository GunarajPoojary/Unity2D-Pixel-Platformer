namespace PixelPlatformer
{
    public class RhinoStateMachine : StateMachine
    {
        public RhinoController Controller { get; }

        public RhinoStateMachine(RhinoController controller)
        {
            Controller = controller;
        }
    }
}