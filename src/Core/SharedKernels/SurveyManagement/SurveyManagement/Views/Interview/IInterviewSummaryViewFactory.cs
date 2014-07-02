using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public interface IInterviewSummaryViewFactory
    {
        InterviewSummary Load(Guid interviewId);
    }
}