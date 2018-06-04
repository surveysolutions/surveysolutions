using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewApprovedByHQ : InterviewActiveEvent
    {
        public string Comment { get; private set; }

        public InterviewApprovedByHQ(Guid userId, string comment, DateTimeOffset originDate)
            : base(userId, originDate)
        {
            this.Comment = comment;
        }
    }
}
