namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class SchedulerSettings
    {
        public SchedulerSettings(bool schedulerEnabled, int hqSynchronizationInterval)
        {
            this.SchedulerEnabled = schedulerEnabled;
            this.HqSynchronizationInterval = hqSynchronizationInterval;
        }

        public bool SchedulerEnabled { get; private set; }
        public int HqSynchronizationInterval { get; private set; }
    }
}