namespace WB.Services.Scheduler.Model.Events
{
    public abstract class JobEvent : IJobEvent
    {
        public long Id { get; }

        protected JobEvent(long jobId)
        {
            Id = jobId;
        }
    }
}
