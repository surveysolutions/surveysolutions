using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Model;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services
{
    interface IJobService
    {
        Task<JobItem> AddNewJobAsync(TenantInfo tenant, JobItem job);
        Task ClearStaleJobs();
        Task CompleteJob(long jobId);
        Task FailJob(long jobId, Exception exception);
        Task<List<JobItem>> GetAllJobs(TenantInfo tenant, params JobStatus[] statuses);
        Task<JobItem> GetFreeJob(CancellationToken token = default);
        Task<JobItem> GetJob(long id);
        Task<bool> LockJob(long jobId);
        Task StartJobAsync(long jobId);
        Task<bool> UnlockJob(long jobId);
        Task UnlockJobs();
        Task UpdateJobAsync(TenantInfo tenant, string tag, Action<JobItem> itemAction);
    }
}
