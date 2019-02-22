using System;
using System.Threading.Tasks;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IInterviewReferencesStorage
    {
        Task<InterviewReference> FindAsync(Guid interviewId);
    }

    class InterviewReferencesStorage : IInterviewReferencesStorage
    {
        private readonly TenantDbContext tenantDbContext;

        public InterviewReferencesStorage(TenantDbContext tenantDbContext)
        {
            this.tenantDbContext = tenantDbContext;
        }

        public Task<InterviewReference> FindAsync(Guid interviewId)
        {
            return this.tenantDbContext.InterviewReferences.FindAsync(interviewId);
        }
    }
}
