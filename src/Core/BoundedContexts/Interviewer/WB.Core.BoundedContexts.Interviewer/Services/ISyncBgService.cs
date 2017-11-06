namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ISyncBgService
    {
        void StartSync();

        SyncProgressDto CurrentProgress { get; }
    }
}