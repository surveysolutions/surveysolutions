namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    public class SyncPackagesProcessorBackgroundJobSetting
    {
        public SyncPackagesProcessorBackgroundJobSetting(bool enabled, int synchronizationInterval, int synchronizationBatchCount, int synchronizationParallelExecutorsCount)
        {
            this.Enabled = enabled;
            this.SynchronizationInterval = synchronizationInterval;
            this.SynchronizationBatchCount = synchronizationBatchCount;
            this.SynchronizationParallelExecutorsCount = synchronizationParallelExecutorsCount;

        }

        public bool Enabled { get; private set; }
        public int SynchronizationInterval { get; private set; }
        public int SynchronizationBatchCount { get; private set; }
        public int SynchronizationParallelExecutorsCount { get; private set; }
    }
}