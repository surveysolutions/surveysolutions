using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IEntitiesListViewModelFactory
    {
        EntitiesListViewModelFactoryResult GetTopEntitiesWithErrors(string interviewId, NavigationState navigationState, bool forSupervisor);
        EntitiesListViewModelFactoryResult GetTopUnansweredQuestions(string interviewId, NavigationState navigationState, bool forSupervisor);
        EntitiesListViewModelFactoryResult GetTopEntitiesWithComments(string interviewId, NavigationState navigationState);
        EntitiesListViewModelFactoryResult GetTopUnansweredCriticalQuestions(string interviewId, NavigationState navigationState);
        EntitiesListViewModelFactoryResult GetTopFailedCriticalRules(string interviewId, NavigationState navigationState);
        EntitiesListViewModelFactoryResult GetTopFailedCriticalRulesFromState(string interviewId, NavigationState navigationState);
        int MaxNumberOfEntities { get; }
    }
}
