using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IExecutedCommandsStorage
    {
        void Add(Guid interviewId, InterviewCommand command);
        List<InterviewCommand> Get(Guid interviewId);
        void Clear(Guid interviewId);
    }
}
