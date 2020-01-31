namespace WB.Services.Scheduler.Model.Events
{
    public class CompleteJobEvent : JobEvent
    {
        public CompleteJobEvent(long jobId) : base(jobId)
        {
        }
    }
}
