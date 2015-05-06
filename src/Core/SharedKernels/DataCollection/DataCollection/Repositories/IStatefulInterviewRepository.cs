using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IStatefulInterviewRepository
    {
        InterviewModel Get(string interviewId);
    }
}