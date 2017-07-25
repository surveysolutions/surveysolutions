using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Services.WebInterview
{
    public interface IWebInterviewNotificationService
    {
        void RefreshEntities(Guid interviewId, params Identity[] questions);
        void ReloadInterview(Guid interviewId);
        void MarkAnswerAsNotSaved(string interviewId, string questionId, string errorMessage);
        void RefreshRemovedEntities(Guid interviewId, params Identity[] entities);
        void FinishInterview(Guid interviewId);
        void RefreshLinkedToRosterQuestions(Guid interviewId, Identity[] rosterIdentities);
        void RefreshEntitiesWithFilteredOptions(Guid interviewId);
        void RefreshLinkedToListQuestions(Guid interviewId, Identity[] identities);
        void RefreshComment(Guid interviewId, Identity question);
    }
}
