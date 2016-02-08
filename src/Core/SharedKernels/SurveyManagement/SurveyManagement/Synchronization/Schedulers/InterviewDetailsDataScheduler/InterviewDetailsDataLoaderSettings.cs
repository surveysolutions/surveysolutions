namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    public class InterviewDetailsDataLoaderSettings
    {
        public InterviewDetailsDataLoaderSettings(bool schedulerEnabled, int synchronizationInterval, int synchronizationBatchCount)
        {
            this.SchedulerEnabled = schedulerEnabled;
            this.SynchronizationInterval = synchronizationInterval;
            this.SynchronizationBatchCount = synchronizationBatchCount;
        }

        public bool SchedulerEnabled { get; private set; }
        public int SynchronizationInterval { get; private set; }
        public int SynchronizationBatchCount { get; private set; }
    }
}