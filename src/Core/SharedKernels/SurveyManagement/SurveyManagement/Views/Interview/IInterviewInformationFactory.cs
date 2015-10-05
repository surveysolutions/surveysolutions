using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public interface IInterviewInformationFactory
    {
        IEnumerable<InterviewInformation> GetInProgressInterviews(Guid interviewerId);
        IEnumerable<InterviewInformation> GetInterviewsByIds(Guid[] interviewIds);
    }
}