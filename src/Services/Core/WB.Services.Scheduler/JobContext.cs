using System.Diagnostics;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Storage;

namespace WB.Services.Scheduler
{
    [DebuggerDisplay("db #{Id}")]
    internal class JobContext : DbContext
    {
        private static long counter = 0;
        public long Id { get; }
        private readonly IOptions<JobSettings> jobSettings;
        
        public JobContext(DbContextOptions<JobContext> options, IOptions<JobSettings> jobSettings)
            : base(options)
        {
            this.jobSettings = jobSettings;
            Id = Interlocked.Increment(ref counter);
        }

        public DbSet<JobItem> Jobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSnakeCaseNaming();
            modelBuilder.HasDefaultSchema(jobSettings.Value.SchemaName);

            modelBuilder.ApplyConfiguration(new JobItemConfiguration());
        }
    }
}
