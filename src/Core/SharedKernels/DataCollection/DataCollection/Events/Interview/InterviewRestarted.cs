using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewRestarted : InterviewActiveEvent
    {
        public InterviewRestarted(Guid userId, DateTimeOffset originDate, string comment, DateTime? restartTime = null)
            : base(userId, originDate)
        {
            Comment = comment;

            if (originDate != default(DateTimeOffset))
            {
                this.RestartTime = originDate.UtcDateTime;
            }
            else if (restartTime != default(DateTime))
            {
                this.RestartTime = restartTime;
            }
        }

        public DateTime? RestartTime { get; private set; }
        public string Comment { get; private set; }
    }
}
