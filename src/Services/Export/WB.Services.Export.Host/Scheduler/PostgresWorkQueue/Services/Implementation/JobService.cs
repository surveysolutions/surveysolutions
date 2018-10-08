using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Model;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services.Implementation
{
    class JobService : IJobService
    {
        private readonly JobContext db;
        private readonly ILogger<JobService> logger;

        public JobService(JobContext db, ILogger<JobService> logger)
        {
            this.db = db;
            this.logger = logger;
        }
        
        public async Task<JobItem> AddNewJobAsync(TenantInfo tenant, JobItem job)
        {
            logger.LogTrace("Adding new job: " + job.Tag);

            var existingJob = await db.Jobs
                .Where(j =>
                    tenant.Id.Id == j.Tenant
                    && j.Tag == job.Tag
                    && (j.Status == JobStatus.Running || j.Status == JobStatus.Created))
                .FirstOrDefaultAsync();

            if (existingJob == null)
            {
                job.CreatedAt = DateTime.UtcNow;
                var newItem = await db.Jobs.AddAsync(job);
                await db.SaveChangesAsync();
                logger.LogTrace("Added new job with id: " + newItem.Entity.Id);

                return newItem.Entity;
            }

            return existingJob;
        }

        public Task<JobItem> GetFreeJob(CancellationToken token = default) => db.GetFreeAsync();

        public Task<JobItem> GetJob(long id) => db.Jobs.FindAsync(id);

        public async Task StartJobAsync(long jobId)
        {
            var job = await GetJob(jobId);
            job.Status = JobStatus.Running;
            job.StartAt = DateTime.UtcNow;
            job.LastUpdateAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        public async Task CompleteJob(long jobId)
        {
            var job = await GetJob(jobId);
            job.Status = JobStatus.Completed;
            job.EndAt = DateTime.UtcNow;
            job.LastUpdateAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        public async Task FailJob(long jobId, Exception exception)
        {
            var job = await GetJob(jobId);
            job.Status = JobStatus.Fail;
            job.EndAt = DateTime.UtcNow;
            job.LastUpdateAt = DateTime.UtcNow;
            job.ErrorMessage = exception.ToString();
            await db.SaveChangesAsync();
        }

        public async Task UpdateJobAsync(TenantInfo tenant, string tag, Action<JobItem> itemAction)
        {
            var job = await GetJob(tenant, tag);
            if (job == null) return;

            itemAction(job);
            job.LastUpdateAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        private async Task<JobItem> GetJob(TenantInfo tenant, string tag)
        {
            return await db.Jobs
                .Where(j => j.Tenant == tenant.Id.Id && j.Tag == tag && (j.Status == JobStatus.Running || j.Status == JobStatus.Created))
                .FirstOrDefaultAsync();
        }

        public Task<List<JobItem>> GetAllJobs(TenantInfo tenant, params JobStatus[] statuses)
        {
            return db.Jobs.Where(j => j.Tenant == tenant.Id.Id
                                      && (statuses.Length == 0 || statuses.Contains(j.Status)))
                .ToListAsync();
        }

        public async Task<bool> LockJob(long jobId)
        {
            return await db.Database.GetDbConnection()
                .QuerySingleAsync<bool>($"select pg_try_advisory_lock ({jobId})");
        }

        public async Task<bool> UnlockJob(long jobId)
        {
            return await db.Database.GetDbConnection()
                .QuerySingleAsync<bool>($"select pg_advisory_unlock ({jobId})");
        }

        public async Task UnlockJobs()
        {
            await db.Database.GetDbConnection()
                .ExecuteAsync("select pg_advisory_unlock_all()");
        }

        public async Task ClearStaleJobs()
        {
            var list = await db.Jobs
                .Where(j => j.Status == JobStatus.Running
                            && j.LastUpdateAt.AddMinutes(5) < DateTime.UtcNow)
                .ToListAsync();

            foreach (var job in list)
            {
                if (await LockJob(job.Id))
                {
                    job.Status = JobStatus.Canceled;
                    job.ErrorMessage = "Canceled due to inactivity";
                }
            }

            await db.SaveChangesAsync();
            await UnlockJobs();
        }
    }
}
