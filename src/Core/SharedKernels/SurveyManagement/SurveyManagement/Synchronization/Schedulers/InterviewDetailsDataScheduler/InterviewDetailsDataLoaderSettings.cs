namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    public class InterviewDetailsDataLoaderSettings
    {
        public InterviewDetailsDataLoaderSettings(bool schedulerEnabled, int synchronizationInterval)
        {
            this.SchedulerEnabled = schedulerEnabled;
            this.SynchronizationInterval = synchronizationInterval;
        }

        public bool SchedulerEnabled { get; private set; }
        public int SynchronizationInterval { get; private set; }
    }
}