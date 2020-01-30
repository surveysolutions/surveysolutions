namespace WB.Services.Scheduler.Model.Events
{
    public class CompleteJobEvent : JobEvent
    {
        public CompleteJobEvent(long jobId) : base(jobId)
        {
        }
    }

    public class ReEnqueueJobEvent : JobEvent
    {
        public ReEnqueueJobEvent(long jobId) : base(jobId)
        {
            
        }
    }
}
