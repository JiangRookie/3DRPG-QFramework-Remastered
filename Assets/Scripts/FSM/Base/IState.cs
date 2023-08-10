namespace QFrameworkRemasteredRPGGame
{
    public interface IState
    {
        void OnEnter();
        void OnLogicUpdate();
        void OnPhysicUpdate();
        void OnExit();
    }
}