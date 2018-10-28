namespace WB.Services.Scheduler.Model.Events
{
    public class StartJobEvent : JobEvent
    {
        public string WorkerId { get; }

        public StartJobEvent(long jobId, string workerId) : base(jobId)
        {
            WorkerId = workerId;
        }
    }
}
