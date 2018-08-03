using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewRestarted : InterviewActiveEvent
    {
        public InterviewRestarted(Guid userId, DateTimeOffset originDate, string comment)
            : base(userId, originDate)
        {
            Comment = comment;

            if (originDate != default(DateTimeOffset))
            {
                this.RestartTime = originDate.UtcDateTime;
            }
        }

        public DateTime? RestartTime { get; set; }
        public string Comment { get; private set; }
    }
}
