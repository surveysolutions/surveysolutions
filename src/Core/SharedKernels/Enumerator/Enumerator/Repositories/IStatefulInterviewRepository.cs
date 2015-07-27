using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Tester.Repositories
{
    public interface IStatefulInterviewRepository
    {
        IStatefulInterview Get(string interviewId);
    }
}