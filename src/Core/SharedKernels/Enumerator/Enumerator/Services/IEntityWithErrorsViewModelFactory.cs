using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IEntitiesListViewModelFactory
    {
        IEnumerable<EntityWithErrorsViewModel> GetTopEntitiesWithErrors(string interviewId, NavigationState navigationState);
        IEnumerable<EntityWithErrorsViewModel> GetTopUnansweredQuestions(string interviewId, NavigationState navigationState);
        IEnumerable<EntityWithCommentsViewModel> GetTopEntitiesWithComments(string interviewId, NavigationState navigationState);
        IEnumerable<EntityWithErrorsViewModel> GetTopUnansweredCriticalQuestions(string interviewId, NavigationState navigationState);
        IEnumerable<FailedCriticalRuleViewModel> GetTopFailedCriticalRules(string interviewId, NavigationState navigationState);
        int MaxNumberOfEntities { get; }
    }
}
