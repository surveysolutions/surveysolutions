using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Infrastructure.Storage;
using WB.Services.Infrastructure.Tenant;
using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class JobService : IJobService
    {
        private readonly JobContext db;
        private readonly IOptions<JobSettings> jobSettings;
        private readonly ILogger<JobService> logger;

        public JobService(JobContext db, IOptions<JobSettings> jobSettings, ILogger<JobService> logger)
        {
            this.db = db;
            this.jobSettings = jobSettings;
            this.logger = logger;
        }

        // Using this lock we ensure that only one thread in all workers cluster can add job
        private static long lock_add_value = -777;

        public async Task<JobItem> AddNewJobAsync(JobItem job)
        {
            logger.LogTrace("Adding new job {jobTag}", job.Tag);

            await using var tr = await db.Database.BeginTransactionAsync();
            await db.AcquireXactLockAsync(lock_add_value);

            var existingJob = await db.Jobs
                .Where(j =>
                    job.Tenant == j.Tenant
                    && j.Tag == job.Tag
                    && (j.Status == JobStatus.Running || j.Status == JobStatus.Created))
                .FirstOrDefaultAsync();

            if (existingJob != null) return existingJob;

            var now = DateTime.UtcNow;
            job.CreatedAt = now;
            job.LastUpdateAt = now;
            
            var newItem = await db.Jobs.AddAsync(job);
            await db.SaveChangesAsync();
            logger.LogTrace("Added new job with id: {jobId}- {jobTag}", newItem.Entity.Id,  job.Tag);

            tr.Commit();

            return newItem.Entity;
        }

        public async Task<JobItem?> GetFreeJobAsync(CancellationToken token = default)
        {
            await using var tr = await db.Database.BeginTransactionAsync(token);
            await db.AcquireXactLockAsync(lock_add_value);

            var running = JobStatus.Running.ToString().ToLowerInvariant();
            var created = JobStatus.Created.ToString().ToLowerInvariant();
            var maxPerTenant = jobSettings.Value.WorkerCountPerTenant;
                
            var schema = jobSettings.Value.SchemaName;

            var newQuery = $@"WITH running_jobs_per_queue AS (
                          select tenant, count(1) AS running_jobs 
                          from {schema}.jobs
                          where (status = '{running}') -- running                            
                          group by 1
                        ),
                        -- find out queues that are full
                        full_queues AS (
                          select R.tenant
                          from running_jobs_per_queue R
                          where R.running_jobs >= {maxPerTenant}
                        )
                        select id
                        from {schema}.jobs
                        where status = '{created}'
                          and tenant NOT IN ( select tenant from full_queues )
                          and (schedule_at is null or schedule_at < (now() at time zone 'utc'))
                        order by id asc
                        for update skip locked
                        limit 1";

            var jobId = await db.Database.GetDbConnection().QuerySingleOrDefaultAsync<long>(newQuery);
            var job = await db.Jobs.FindAsync(jobId);
               
            if (job != null)
            {
                job.Start(jobSettings.Value.WorkerId);
                db.Jobs.Update(job);
            }

            await db.SaveChangesAsync(token);

            tr.Commit();
            return job;
        }
        
        public async Task<bool> HasMostRecentFinishedJobIdWithSameTag(long jobId, TenantInfo tenant)
        {
            var job = await this.GetJobAsync(jobId);
            if(job == null || job.Tenant != tenant.Id.Id) 
                throw new ArgumentException("Cannot found job id: " + jobId, nameof(jobId));

            bool hasMoreRecentJob = await db.Jobs
                .Where(x => x.Tag == job.Tag && x.Status == JobStatus.Completed && x.Id > jobId)
                .AnyAsync();

            return hasMoreRecentJob;
        }

        public ValueTask<JobItem?> GetJobAsync(long id)
        {
            return db.Jobs.FindAsync(id);
        }

        public Task<List<JobItem>> GetJobsAsync(long[] ids)
        {
            return db.Jobs.Where(j => ids.Contains(j.Id)).ToListAsync();
        }

        public Task<JobItem?> GetJobAsync(TenantInfo tenant, string tag)
        {
            return db.Jobs
                .Where(j => j.Tenant == tenant.Id.Id && j.Tag == tag)
                .FirstOrDefaultAsync();
        }

        public Task<List<JobItem>> GetAllJobsAsync(TenantInfo tenant)
        {
            return db.Jobs.Where(j => j.Tenant == tenant.Id.Id).ToListAsync();
        }

        public Task<List<JobItem>> GetRunningOrQueuedJobs(TenantInfo tenant)
        {
            return db.Jobs.Where(j => j.Tenant == tenant.Id.Id &&
                                      (j.Status == JobStatus.Created || j.Status == JobStatus.Running))
                .ToListAsync();
        }
    }
}
