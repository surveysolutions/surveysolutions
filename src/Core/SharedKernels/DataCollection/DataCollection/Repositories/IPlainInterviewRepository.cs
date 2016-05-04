using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IPlainInterviewRepository
    {
        InterviewModel GetInterview(string interviewId);
        void StoreInterview(InterviewModel interview, string interviewId);
        void DeleteInterview(string interviewId);
    }
}