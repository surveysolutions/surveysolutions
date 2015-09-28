using System;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
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
            this.changeLogManipulator.CloseDraftRecord(interviewId, userId);
        }
    }
}
