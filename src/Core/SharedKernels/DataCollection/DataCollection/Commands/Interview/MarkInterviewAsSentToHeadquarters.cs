using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class MarkInterviewAsSentToHeadquarters : InterviewCommand
    {
        public MarkInterviewAsSentToHeadquarters(Guid interviewId, Guid userId)
            : base(interviewId, userId) {}
    }
}