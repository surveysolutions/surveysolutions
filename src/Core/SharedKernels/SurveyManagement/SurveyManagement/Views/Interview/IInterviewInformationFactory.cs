using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public interface IInterviewInformationFactory
    {
        IEnumerable<InterviewInformation> GetInProgressInterviews(Guid interviewerId);
        IEnumerable<InterviewInformation> GetInterviewsByIds(Guid[] interviewIds);
        InterviewSynchronizationDto GetInterview(Guid interviewId);
    }
}