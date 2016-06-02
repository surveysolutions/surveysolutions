using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewDetailsViewFactory
    {
        DetailsViewModel GetInterviewDetails(Guid interviewId, Guid? currentGroupId = null,
            decimal[] currentGroupRosterVector = null, InterviewDetailsFilter? filter = null);

        Guid? GetFirstChapterId(Guid interviewId);
    }
}