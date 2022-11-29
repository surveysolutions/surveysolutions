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
        }

        [Obsolete("Please use OriginDate property")]
        public DateTime? RestartTime { get; set; }
        public string Comment { get; private set; }
    }
}
