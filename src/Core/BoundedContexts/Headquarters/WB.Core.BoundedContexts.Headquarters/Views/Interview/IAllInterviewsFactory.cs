using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IAllInterviewsFactory
    {
        AllInterviewsView Load(AllInterviewsInputModel input);
        InterviewsWithoutPrefilledView LoadInterviewsWithoutPrefilled(InterviewsWithoutPrefilledInputModel input);

        InterviewSummary Load(Guid interviewId);
    }
}
