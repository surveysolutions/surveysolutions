using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public interface IInterviewDetailsViewFactory
    {
        DetailsViewModel GetInterviewDetails(Guid interviewId, Guid? currentGroupId = null,
            decimal[] currentGroupRosterVector = null, InterviewDetailsFilter? filter = null);
    }
}