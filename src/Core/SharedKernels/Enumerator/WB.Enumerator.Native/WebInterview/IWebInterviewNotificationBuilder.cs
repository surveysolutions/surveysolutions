using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview.LifeCycle;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IWebInterviewNotificationBuilder
    {
        void RefreshEntitiesWithFilteredOptions(InterviewLifecycle cycle, Guid interviewId);
        void RefreshCascadingOptions(InterviewLifecycle cycle, Guid interviewId, Identity identity);
        void RefreshLinkedToListQuestions(InterviewLifecycle cycle, Guid interviewId, Identity questionIdentity);
        void RefreshLinkedToRosterQuestions(InterviewLifecycle cycle, Guid interviewId, Identity[] rosterIdentities);
    }
}
