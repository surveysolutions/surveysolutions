using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public interface IInterviewerInterviewsFactory
    {
        IEnumerable<InterviewerInterview> GetInProgressInterviews(Guid interviewerId);
    }
}