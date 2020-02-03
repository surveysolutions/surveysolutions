namespace WB.Services.Scheduler.Model.Events
{
    public class ReEnqueueJobEvent : JobEvent
    {
        public ReEnqueueJobEvent(long jobId) : base(jobId)
        {
            
        }
    }
}
