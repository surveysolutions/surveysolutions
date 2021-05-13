using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class SupervisorInterviewStateCalculationStrategy : IInterviewStateCalculationStrategy
    {
        public InterviewSimpleStatus GetInterviewSimpleStatus(IStatefulInterview interview)
        {
            return interview.GetInterviewSimpleStatus(true);
        }
    }
}
