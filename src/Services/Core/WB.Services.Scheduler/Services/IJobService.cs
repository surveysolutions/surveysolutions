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
        Task<List<JobItem>> GetAllJobsAsync(TenantInfo tenant, params JobStatus[] statuses);
        Task<JobItem> GetFreeJobAsync(CancellationToken token = default);
        Task<JobItem> GetJobAsync(TenantInfo tenant, string tag, params JobStatus[] statuses);
    }
}
