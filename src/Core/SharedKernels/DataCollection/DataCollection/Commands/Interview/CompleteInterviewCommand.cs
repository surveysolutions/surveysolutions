using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CompleteInterviewCommand : InterviewCommand
    {
        public CompleteInterviewCommand(Guid interviewId, Guid userId, string comment, CriticalLevel? criticalLevel)
            : base(interviewId, userId)
        {
            this.Comment = comment;
            this.CriticalLevel = criticalLevel;
        }

        public string Comment { get; set; }
        public CriticalLevel? CriticalLevel { get; }
    }
}
