using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Repositories
{
    public interface IStatefullInterviewRepository
    {
        IStatefulInterview Get(string interviewId);
    }
}