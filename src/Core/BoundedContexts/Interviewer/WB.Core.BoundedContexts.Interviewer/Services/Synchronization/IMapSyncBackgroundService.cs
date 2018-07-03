namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IMapSyncBackgroundService
    {
        void SyncMaps();

        MapSyncProgressStatus CurrentProgress { get; }
    }
}