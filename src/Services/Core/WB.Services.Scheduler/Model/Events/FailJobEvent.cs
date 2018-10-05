using System;

namespace WB.Services.Scheduler.Model.Events
{
    public class FailJobEvent : JobEvent
    {
        public Exception Exception { get; }

        public FailJobEvent(long jobId, Exception exception) : base(jobId)
        {
            Exception = exception;
        }
    }
}
