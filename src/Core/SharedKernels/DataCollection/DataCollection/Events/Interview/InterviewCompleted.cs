using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewCompleted : InterviewActiveEvent
    {
        public InterviewCompleted(Guid userId, DateTimeOffset originDate, string comment)
            : base(userId, originDate)
        {
            this.Comment = comment;

            if (originDate != default(DateTimeOffset))
                this.CompleteTime = originDate.UtcDateTime;
            
        }

        public DateTime? CompleteTime { get; set; }
        public string Comment { get; private set; }
    }
}
