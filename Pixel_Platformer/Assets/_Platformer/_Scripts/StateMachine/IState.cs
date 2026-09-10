namespace PixelPlatformer
{
    public interface IState
    {
        void Enter();
        void UpdateState();
        void Exit();
    }
}