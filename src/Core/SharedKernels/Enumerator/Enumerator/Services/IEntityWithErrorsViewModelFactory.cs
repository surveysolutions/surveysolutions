using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IEntitiesListViewModelFactory
    {
        IEnumerable<EntityWithErrorsViewModel> GetEntitiesWithErrors(string interviewId, NavigationState navigationState);
        IEnumerable<EntityWithErrorsViewModel> GetUnansweredQuestions(string interviewId, NavigationState navigationState);
        IEnumerable<EntityWithCommentsViewModel> GetEntitiesWithComments(string interviewId, NavigationState navigationState);
        IEnumerable<EntityWithErrorsViewModel> GetUnansweredCriticalQuestions(string interviewId, NavigationState navigationState);
        IEnumerable<FailCriticalityConditionViewModel> GetTopFailedCriticalRules(string interviewId, NavigationState navigationState);
        int MaxNumberOfEntities { get; }
        bool HasCriticalFeature(string interviewId);
    }
}
