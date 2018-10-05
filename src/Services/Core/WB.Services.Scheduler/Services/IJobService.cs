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
        Task<List<JobItem>> GetAllJobs(TenantInfo tenant, params JobStatus[] statuses);
        Task<JobItem> GetFreeJobAsync(CancellationToken token = default);
        Task<JobItem> GetJob(TenantInfo tenant, string tag);
    }
}
