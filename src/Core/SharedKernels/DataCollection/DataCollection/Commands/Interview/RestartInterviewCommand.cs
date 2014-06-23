using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof (Implementation.Aggregates.Interview), "Restart")]
    public class RestartInterviewCommand : InterviewCommand
    {
        public RestartInterviewCommand(Guid interviewId, Guid userId, string comment, DateTime restartTime)
            : base(interviewId, userId)
        {
            this.RestartTime = restartTime;
            this.Comment = comment;
        }

        public string Comment { get; set; }
        public DateTime RestartTime { get; private set; }
    }
}