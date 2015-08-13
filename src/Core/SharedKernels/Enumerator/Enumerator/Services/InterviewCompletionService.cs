using System;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IInterviewCompletionService
    {
        void CompleteInterview(Guid interviewId, Guid userId);
    }
}
