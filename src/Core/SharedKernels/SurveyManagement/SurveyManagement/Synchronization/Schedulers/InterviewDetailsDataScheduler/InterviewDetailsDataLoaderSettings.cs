namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    public class InterviewDetailsDataLoaderSettings
    {
        public InterviewDetailsDataLoaderSettings(bool schedulerEnabled, int synchronizationInterval, int synchronizationBatchCount, int synchronizationParallelExecutorsCount)
        {
            this.SchedulerEnabled = schedulerEnabled;
            this.SynchronizationInterval = synchronizationInterval;
            this.SynchronizationBatchCount = synchronizationBatchCount;
            this.SynchronizationParallelExecutorsCount = synchronizationParallelExecutorsCount;

        }

        public bool SchedulerEnabled { get; private set; }
        public int SynchronizationInterval { get; private set; }
        public int SynchronizationBatchCount { get; private set; }
        public int SynchronizationParallelExecutorsCount { get; private set; }
    }
}