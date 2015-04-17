using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class PlainInterviewRepository : IPlainInterviewRepository
    {
        private readonly IPlainStorageAccessor<InterviewModel> repository;

        public PlainInterviewRepository(IPlainStorageAccessor<InterviewModel> repository)
        {
            this.repository = repository;
        }

        public InterviewModel GetInterview(string interviewId)
        {
            return this.repository.GetById(interviewId);
        }

        public void StoreInterview(InterviewModel interview, string interviewId)
        {
            this.repository.Store(interview, interviewId);
        }

        public void DeleteInterview(string interviewId)
        {
            this.repository.Remove(interviewId);
        }
    }
}