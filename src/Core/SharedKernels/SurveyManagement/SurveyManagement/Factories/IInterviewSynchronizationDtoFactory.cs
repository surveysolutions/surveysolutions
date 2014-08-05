using System;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Factories
{
    public interface IInterviewSynchronizationDtoFactory 
    {
        InterviewSynchronizationDto BuildFrom(InterviewData interview, Guid userId, InterviewStatus status, string comments);
        InterviewSynchronizationDto BuildFrom(InterviewData interview, string comments);
    }
}