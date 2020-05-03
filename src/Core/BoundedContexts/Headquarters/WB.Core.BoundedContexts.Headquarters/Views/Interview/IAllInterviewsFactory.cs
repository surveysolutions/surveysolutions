using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IAllInterviewsFactory
    {
        AllInterviewsView Load(AllInterviewsInputModel input);

        InterviewSummary Load(Guid interviewId);
    }
}
