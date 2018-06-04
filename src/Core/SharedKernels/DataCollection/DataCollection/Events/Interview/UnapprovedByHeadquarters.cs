using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class UnapprovedByHeadquarters : InterviewActiveEvent
    {
        public string Comment { get; private set; }

        public UnapprovedByHeadquarters(Guid userId, string comment, DateTimeOffset originDate)
            : base(userId, originDate)
        {
            this.Comment = comment;
        }
    }
}
