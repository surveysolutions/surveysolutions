namespace WB.Core.BoundedContexts.Headquarters.Synchronization
{
    public class InterviewDetailsDataLoaderSettings
    {
        public InterviewDetailsDataLoaderSettings(bool schedulerEnabled, int synchronizationInterval,
            int numberOfInterviewsProcessedAtTime)
        {
            this.SchedulerEnabled = schedulerEnabled;
            this.SynchronizationInterval = synchronizationInterval;
            this.NumberOfInterviewsProcessedAtTime = numberOfInterviewsProcessedAtTime;
        }

        public bool SchedulerEnabled { get; private set; }
        public int SynchronizationInterval { get; private set; }
        public int NumberOfInterviewsProcessedAtTime { get; private set; }
    }
}