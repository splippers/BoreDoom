namespace Chorewars.Modes
{
    public interface IChoreMode
    {
        string ModeId { get; }
        string DisplayName { get; }

        void Begin();
        void End();
    }
}
