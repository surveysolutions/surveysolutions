using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewStateCalculationStrategy
    {
        InterviewSimpleStatus GetInterviewSimpleStatus(IStatefulInterview interview);
    }
}
