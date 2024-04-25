using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CompleteInterviewCommand : InterviewCommand
    {
        public CompleteInterviewCommand(Guid interviewId, Guid userId, string comment, CriticalityLevel? criticalityLevel)
            : base(interviewId, userId)
        {
            this.Comment = comment;
            this.CriticalityLevel = criticalityLevel;
        }

        public string Comment { get; set; }
        public CriticalityLevel? CriticalityLevel { get; }
    }
}
