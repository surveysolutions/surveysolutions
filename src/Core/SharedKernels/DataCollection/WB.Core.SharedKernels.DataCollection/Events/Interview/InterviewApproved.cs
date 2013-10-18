using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewApproved : InterviewActiveEvent
    {
        public string Comment { get; private set; }

        public InterviewApproved(Guid userId, string comment)
            : base(userId)
        {
            this.Comment = comment;
        }
    }
}