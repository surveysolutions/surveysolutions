using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Model;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue
{
    class JobContext : DbContext
    {
        private readonly IOptions<JobSettings> jobSettings;

        public JobContext(DbContextOptions<JobContext> options, IOptions<JobSettings> jobSettings)
            : base(options)
        {
            this.jobSettings = jobSettings;
        }

        public DbSet<JobItem> Jobs { get; set; }

        [SuppressMessage("Possible SQL injection vulnerability", "EF1000")]
        public async Task<JobItem> GetFreeAsync()
        {
            var status = JobStatus.Created.ToString().ToLowerInvariant();

            // we don't want to spam single tenant with multiple parallel export jobs
            // so filter out those that are already running
            var runningQuery = $"select count(jt.id) from {jobSettings.Value.SchemaName}.jobs jt " +
                               "where j.tenant = jt.tenant " +
                               $"and jt.status = '{JobStatus.Running.ToString().ToLowerInvariant()}'";

            var query = $"select j.id from {jobSettings.Value.SchemaName}.jobs j " +

                        // limit amount of work done per tenant
                        $"where status = '{status}' and ({runningQuery}) < {jobSettings.Value.WorkerCountPerTenant} " +
                        
                        // filter out scheduled tasks that are yet to start
                        $"and (j.schedule_at is null or j.schedule_at < '{DateTime.UtcNow}')" +

                        // `for update skip locked` will ensure that only non locked jobs to return
                        $" order by id asc for update skip locked ";

            var jobId = await this.Database.GetDbConnection()
                .QueryFirstOrDefaultAsync<long>(query);

            return await Jobs.FindAsync(jobId);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSnakeCaseNaming();
            modelBuilder.HasDefaultSchema(jobSettings.Value.SchemaName);
            modelBuilder.ApplyConfiguration(new JobItemConfiguration());
        }
    }
}
