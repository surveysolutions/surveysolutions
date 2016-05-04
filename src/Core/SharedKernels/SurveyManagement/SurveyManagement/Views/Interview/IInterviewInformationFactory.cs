using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public interface IInterviewInformationFactory
    {
        IEnumerable<InterviewInformation> GetInProgressInterviews(Guid interviewerId);
        IEnumerable<InterviewInformation> GetInterviewsByIds(Guid[] interviewIds);
        InterviewSynchronizationDto GetInterviewDetails(Guid interviewId);
    }
}