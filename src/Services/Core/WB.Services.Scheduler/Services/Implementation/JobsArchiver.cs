using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Services.Implementation
{
    class JobsArchiver : IJobsArchiver
    {
        private readonly JobContext context;
        private readonly ILogger<JobArchive> logger;

        public JobsArchiver(JobContext context, ILogger<JobArchive> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<int> ArchiveJobs(string tenantName)
        {
            int archiveCounter = 0;

            do
            {
                var jobs = await this.context.Jobs
                    .Where(j => j.TenantName == tenantName)
                    .Take(100).ToListAsync();

                if (jobs.Count == 0)
                {
                    break;
                }

                var archives = jobs.Select(j => new JobArchive
                {
                    Id = j.Id,
                    Tenant = j.Tenant,
                    TenantName = j.TenantName,
                    Args = j.Args,
                    CreatedAt = j.CreatedAt,
                    EndAt = j.EndAt,
                    LastUpdateAt = j.LastUpdateAt,
                    ScheduleAt = j.ScheduleAt,
                    StartAt = j.StartAt,
                    Status = j.Status,
                    Type = j.Type
                });

                await this.context.Archive.AddRangeAsync(archives);
                this.context.Jobs.RemoveRange(jobs);
                await this.context.SaveChangesAsync();
                logger.LogInformation("Archived {count} scheduled jobs for tenant {tenant}", jobs.Count, tenantName);
                archiveCounter += jobs.Count;

            } while (true);

            logger.LogInformation("All {count} scheduled jobs for {tenant} is archived",
                archiveCounter, tenantName);

            return archiveCounter;
        }
    }
}
