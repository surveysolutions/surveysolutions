using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IEntitiesListViewModelFactory
    {
        IEnumerable<EntityWithErrorsViewModel> GetEntitiesWithErrors(string interviewId, NavigationState navigationState);
        IEnumerable<EntityWithCommentsViewModel> GetEntitiesWithComments(string interviewId, NavigationState navigationState);
        int MaxNumberOfEntities { get; }
    }
}