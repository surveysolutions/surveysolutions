namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization
{
    public interface ISyncBgService
    {
        void StartSync();

        SyncProgressDto CurrentProgress { get; }
    }
}
