#nullable enable
using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class WebInterviewRequested : InterviewActiveEvent
    {
        public WebInterviewRequested(Guid userId, DateTimeOffset originDate, string comment)
            : base(userId, originDate)
        {
            this.Comment = comment;
            this.CompleteTime = originDate.UtcDateTime;
        }

        public DateTime? CompleteTime { get; }
        public string Comment { get; }
    }
}
