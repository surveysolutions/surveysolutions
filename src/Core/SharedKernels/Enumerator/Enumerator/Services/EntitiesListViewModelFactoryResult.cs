using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services;

public record EntitiesListViewModelFactoryResult(IEnumerable<EntityWithErrorsViewModel> Entities, int Total);

