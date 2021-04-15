using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RequestCapiModeCommand : InterviewCommand
    {
        public string Comment { get; }

        public RequestCapiModeCommand(Guid interviewId, Guid userId, string comment) : base(interviewId, userId)
        {
            Comment = comment;
        }
    }
}
