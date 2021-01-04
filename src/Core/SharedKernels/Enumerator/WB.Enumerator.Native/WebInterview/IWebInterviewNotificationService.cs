using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IWebInterviewNotificationService
    {
        void RefreshEntities(Guid interviewId, params Identity[] questions);
        void ReloadInterview(Guid interviewId);
        void MarkAnswerAsNotSaved(Guid interviewId, Identity questionId, string errorMessage);
        void MarkAnswerAsNotSaved(Guid interviewId, Identity questionId, Exception exception);
        void RefreshRemovedEntities(Guid interviewId, params Identity[] entities);
        void FinishInterview(Guid interviewId);
    }
}
