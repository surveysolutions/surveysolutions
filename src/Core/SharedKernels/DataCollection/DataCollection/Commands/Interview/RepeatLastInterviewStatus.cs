using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RepeatLastInterviewStatus : CommandBase
    {
        public RepeatLastInterviewStatus(Guid interviewId, string comment)
            : base(interviewId)
        {
            this.InterviewId = interviewId;
            this.Comment = comment;
        }

        public Guid InterviewId { get; private set; }
        public string Comment { get; private set; }
    }
}