namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync
{
    public interface ISynchronizationMode
    {
        void Set(SynchronizationMode mode);
        SynchronizationMode GetMode();
    }
}
