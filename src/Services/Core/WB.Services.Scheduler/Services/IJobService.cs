using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Infrastructure.Tenant;
using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Services
{
    public interface IJobService
    {
        Task<JobItem> AddNewJobAsync(JobItem job);
        Task<List<JobItem>> GetAllJobsAsync(TenantInfo tenant);
        Task<List<JobItem>> GetRunningOrQueuedJobs(TenantInfo tenant);
        Task<JobItem?> GetFreeJobAsync(CancellationToken token = default);
        Task<JobItem?> GetJobAsync(TenantInfo tenant, string tag);
        ValueTask<JobItem?> GetJobAsync(long id);
        Task<List<JobItem>> GetJobsAsync(long[] ids);
        Task<bool> HasMostRecentFinishedJobIdWithSameTag(long jobId, TenantInfo tenant);
    }
}
