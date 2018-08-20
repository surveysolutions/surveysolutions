using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IGroupStateCalculationStrategy
    {
        GroupStatus CalculateDetailedStatus(Identity groupIdentity, IStatefulInterview interview);
    }

    public interface IEnumeratorGroupStateCalculationStrategy : IGroupStateCalculationStrategy
    {
    }

    public interface ISupervisorGroupStateCalculationStrategy : IGroupStateCalculationStrategy
    {
    }
}
