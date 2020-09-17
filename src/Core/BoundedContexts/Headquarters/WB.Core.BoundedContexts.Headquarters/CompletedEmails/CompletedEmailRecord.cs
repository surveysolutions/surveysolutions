#nullable enable

using System;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class CompletedEmailRecord
    {
        public virtual Guid InterviewId { get; set; }
        public virtual DateTime RequestTime { get; set; }
        public virtual int FailedCount { get; set; }
    }
}