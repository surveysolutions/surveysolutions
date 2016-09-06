using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface ICompositeCollectionInflationService
    {
        CompositeCollection<ICompositeEntity> GetInflatedCompositeCollection(List<IInterviewEntityViewModel> newGroupItems);
    }
}
