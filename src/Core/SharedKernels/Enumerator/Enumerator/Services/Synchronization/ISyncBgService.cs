namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface ISyncBgService
    {
        void StartSync();

        SyncProgressDto CurrentProgress { get; }
    }
}
