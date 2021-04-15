using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class EnumeratorInterviewStateCalculationStrategy : IInterviewStateCalculationStrategy
    {
        public InterviewSimpleStatus GetInterviewSimpleStatus(IStatefulInterview interview)
        {
            return interview.GetInterviewSimpleStatus(false);
        }
    }
}
