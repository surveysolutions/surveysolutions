using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof (Implementation.Aggregates.Interview), "Complete")]
    public class CompleteInterviewCommand : InterviewCommand
    {
        public CompleteInterviewCommand(Guid interviewId, Guid userId, string comment, DateTime completeTime)
            : base(interviewId, userId)
        {
            this.CompleteTime = completeTime;
            this.Comment = comment;
        }

        public string Comment { get; set; }
        public DateTime CompleteTime { get; private set; }
    }
}