namespace WB.Core.SharedKernels.Enumerator.Services.MapSynchronization
{
    public interface IMapSyncBackgroundService
    {
        void SyncMaps();

        MapSyncProgressStatus CurrentProgress { get; }
    }
}
