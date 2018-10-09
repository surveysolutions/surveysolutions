namespace WB.Services.Scheduler.Model.Events
{
    public class CancelJobEvent : JobEvent
    {
        public string Reason { get; }

        public CancelJobEvent(long jobId, string reason) : base(jobId)
        {
            this.Reason = reason;
        }


        public override string ToString()
        {
            return $"{nameof(CancelJobEvent)}";
        }
    }
}
