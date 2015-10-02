using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewApprovedByHQRevoked : InterviewActiveEvent
    {
        public string Comment { get; private set; }

        public InterviewApprovedByHQRevoked(Guid userId, string comment)
            : base(userId)
        {
            this.Comment = comment;
        }
    }
}