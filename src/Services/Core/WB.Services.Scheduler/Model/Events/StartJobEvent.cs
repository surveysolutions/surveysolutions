namespace WB.Services.Scheduler.Model.Events
{
    public class StartJobEvent : JobEvent
    {
        public StartJobEvent(long jobId) : base(jobId)
        {
        }
    }
}
