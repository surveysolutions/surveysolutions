using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IStatefullInterviewRepository
    {
        IStatefullInterview Get(string interviewId);
    }
}