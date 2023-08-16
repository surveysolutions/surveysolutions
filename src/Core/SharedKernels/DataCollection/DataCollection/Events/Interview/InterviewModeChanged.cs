#nullable enable
using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewModeChanged : InterviewActiveEvent
    {
        public InterviewModeChanged(Guid userId, DateTimeOffset originDate, InterviewMode mode, string? comment = null)
            : base(userId, originDate)
        {
            this.Mode = mode;
            this.Comment = comment;
        }
        
        public InterviewMode Mode { get; }
        public string? Comment { get; }
    }
}
