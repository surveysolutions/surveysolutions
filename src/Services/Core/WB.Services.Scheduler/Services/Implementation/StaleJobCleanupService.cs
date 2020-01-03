using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Infrastructure.Storage;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Storage;

namespace WB.Services.Scheduler.Services.Implementation
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

        public const long CleanupServiceLock = -888;

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await using var tr = await db.Database.BeginTransactionAsync(cancellationToken);
            // do not run multiple cleanups at once
            if (!await db.TryAcquireLockAsync(CleanupServiceLock)) return;

            var list = await db.Jobs.Where(j => j.Status == JobStatus.Running
                                                && j.LastUpdateAt.AddSeconds(options.Value
                                                    .ClearStaleJobsInSeconds) < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var job in list)
            {
                // each job while running - hold a lock, is we cannot acquire it - job is still running
                if (!await db.TryAcquireLockAsync(job.Id)) continue;

                try
                {
                    job.Cancel("Canceled due to inactivity");
                    logger.LogInformation("Job #{jobId} '{jobTag}' marked as canceled due to inactivity", job.Id, job.Tag);
                }
                finally
                {
                    await db.ReleaseLockAsync(job.Id);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);
        }
    }
}
