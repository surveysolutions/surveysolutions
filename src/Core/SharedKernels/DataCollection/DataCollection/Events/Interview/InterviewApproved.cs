using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewApproved : InterviewActiveEvent
    {
        public string Comment { get; private set; }
        
        [Obsolete("Please use OriginDate property")]
        public DateTime? ApproveTime { get; set; }
        public InterviewApproved(Guid userId, string comment, DateTimeOffset originDate) 
            : base(userId, originDate)
        {
            this.Comment = comment;
        }
    }
}
