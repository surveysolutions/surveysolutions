using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IEntityWithErrorsViewModelFactory
    {
        IEnumerable<EntityWithErrorsViewModel> GetEntities(string interviewId, NavigationState navigationState);
    }
}