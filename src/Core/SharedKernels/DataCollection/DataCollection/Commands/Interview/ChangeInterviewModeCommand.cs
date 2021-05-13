#nullable enable
using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ChangeInterviewModeCommand : InterviewCommand
    {
        public InterviewMode Mode { get; }
        public string? Comment { get; }

        public ChangeInterviewModeCommand(Guid interviewId, Guid userId, InterviewMode mode, string? comment = null) 
            : base(interviewId, userId)
        {
            Mode = mode;
            Comment = comment;
        }
    }
}
