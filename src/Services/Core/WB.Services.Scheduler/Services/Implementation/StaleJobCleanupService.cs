using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Storage;

namespace WB.Services.Scheduler.Jobs
{
    class StaleJobCleanupService
    {
        private readonly JobContext db;
        private readonly IOptions<JobSettings> options;
        private readonly ILogger<StaleJobCleanupService> logger;

        public StaleJobCleanupService(JobContext db, IOptions<JobSettings> options, ILogger<StaleJobCleanupService> logger)
        {
            this.db = db;
            this.options = options;
            this.logger = logger;
        }

        public const string Name = "cleanup";
        public const long CleanupServiceLock = -5555;

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var tr = await db.Database.BeginTransactionAsync(cancellationToken))
            {
                // do not run multiple cleanups at once
                if (!await db.TryAcquireLockAsync(CleanupServiceLock)) return;

                var list = await db.Jobs.Where(j => j.Status == JobStatus.Running
                                                    && j.LastUpdateAt.AddSeconds(options.Value
                                                        .ClearStaleJobsInSeconds) < DateTime.UtcNow)
                    .ToListAsync(cancellationToken);

                foreach (var job in list)
                {
                    // each job while running - hold a lock, is we cannot acquire it - job is still running
                    if (await db.TryAcquireLockAsync(job.Id))
                    {
                        job.Cancel("Canceled due to inactivity");
                        logger.LogInformation($"Job {job.Tag} marked as canceled due to inactivity");
                    }

                    await db.ReleaseLockAsync(job.Id);
                }

                await db.SaveChangesAsync(cancellationToken);
                tr.Commit();
            }
        }

        //static readonly JobStatus[] StatusesToRemove = new[] { JobStatus.Completed, JobStatus.Canceled, JobStatus.Fail };
        //private async Task ClearOldJobs(CancellationToken cancellationToken)
        //{
        //    var jobs = await db.Jobs.Where(j =>
        //        StatusesToRemove.Contains(j.Status) && DateTime.UtcNow.AddHours(-8) < j.EndAt).ToListAsync(cancellationToken);

        //    db.Jobs.RemoveRange(jobs);
        //}
    }
}
