using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewSummaryViewFactory
    {
        InterviewSummary Load(Guid interviewId);
    }
}