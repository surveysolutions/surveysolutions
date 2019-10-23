using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

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
        void RefreshLinkedToRosterQuestions(Guid interviewId, Identity[] rosterIdentities);
        void RefreshEntitiesWithFilteredOptions(Guid interviewId);
        void RefreshCascadingOptions(Guid interviewId, Identity identity);
        void RefreshLinkedToListQuestions(Guid interviewId, Identity[] identities);
        void ShutDownInterview(Guid interviewId);
    }
}
