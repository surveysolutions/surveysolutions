using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewRestarted : InterviewActiveEvent
    {
        public InterviewRestarted(Guid userId, DateTime? restartTime, string comment)
            : base(userId)
        {
            Comment = comment;
            if (restartTime != default(DateTime))
            {
                this.RestartTime = restartTime;
            }
        }

        public DateTime? RestartTime { get; private set; }
        public string Comment { get; private set; }
    }
}