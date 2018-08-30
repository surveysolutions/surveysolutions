using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewStateCalculationStrategy
    {
        SimpleGroupStatus CalculateSimpleStatus(IStatefulInterview interview);
        GroupStatus CalculateDetailedStatus(IStatefulInterview interview);
    }
}
