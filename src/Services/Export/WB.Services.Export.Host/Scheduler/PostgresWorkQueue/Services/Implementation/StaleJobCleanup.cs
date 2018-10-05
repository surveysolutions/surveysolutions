using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Model;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services.Implementation
{
    class StaleJobCleanup : IStaleJobCleanup
    {
        private readonly IJobService jobService;
        private readonly JobContext db;

        public StaleJobCleanup(IJobService jobService, JobContext db)
        {
            this.jobService = jobService;
            this.db = db;
        }

        public async Task ExecuteAsync(bool args, CancellationToken cancellationToken)
        {
            await jobService.ClearStaleJobs();
            await ScheduleIfNeeded();
        }

        public async Task ScheduleIfNeeded()
        {
            if (await jobService.LockJob(-1))
            {
                try
                {
                    var existingJob = await db.Jobs
                        .Where(j =>
                            j.Type == JobType.Cleanup &&
                            j.Status == JobStatus.Created)
                        .ToListAsync();

                    if (!existingJob.Any())
                    {
                        await db.Jobs.AddAsync(new JobItem
                        {
                            Type = JobType.Cleanup,
                            Tenant = "ExportService",
                            Args = "false",
                            ScheduleAt = DateTime.UtcNow.AddMinutes(5)
                        });

                        await db.SaveChangesAsync();
                    }
                }
                finally
                {
                    await jobService.UnlockJob(-1);
                }
            }
        }
    }
}
