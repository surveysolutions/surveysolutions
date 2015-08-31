using System;

using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Capi.Implementation.Services
{
    public class InterviewerInterviewCompletionService : IInterviewCompletionService
    {
        readonly IChangeLogManipulator changeLogManipulator;

        public InterviewerInterviewCompletionService(IChangeLogManipulator changeLogManipulator)
        {
            this.changeLogManipulator = changeLogManipulator;
        }

        public void CompleteInterview(Guid interviewId, Guid userId)
        {
            changeLogManipulator.CloseDraftRecord(interviewId, userId);
        }
    }
}
