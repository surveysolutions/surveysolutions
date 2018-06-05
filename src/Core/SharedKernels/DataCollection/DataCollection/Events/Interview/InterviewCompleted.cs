using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewCompleted : InterviewActiveEvent
    {
        public InterviewCompleted(Guid userId, DateTimeOffset originDate, string comment, DateTime? completeTime = null)
            : base(userId, originDate)
        {
            this.Comment = comment;

            if (completeTime!= null && completeTime != default(DateTime))
            {
                this.CompleteTime = completeTime;
            }
        }

        public DateTime? CompleteTime { get; private set; }
        public string Comment { get; private set; }
    }
}
