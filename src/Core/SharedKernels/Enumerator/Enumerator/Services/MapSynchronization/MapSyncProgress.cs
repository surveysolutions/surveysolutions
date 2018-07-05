namespace WB.Core.SharedKernels.Enumerator.Services.MapSynchronization
{
    public class MapSyncProgress
    {
        public bool IsRunning => this.Status == MapSyncStatus.Download ||
                                 this.Status == MapSyncStatus.Started;

        public string Title { get; set; }
        public string Description { get; set; }
        public int TotalMapsCount { get; set; }

        public MapSyncStatus Status { get; set; }
    }
}
