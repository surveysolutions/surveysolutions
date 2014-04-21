namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class SchedulerSettings
    {
        public SchedulerSettings(int hqSynchronizationInterval)
        {
            this.HqSynchronizationInterval = hqSynchronizationInterval;
        }

        public int HqSynchronizationInterval { get; set; }
    }
}