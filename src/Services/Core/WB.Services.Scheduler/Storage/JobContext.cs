using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Storage
{
    internal class JobContext : DbContext
    {
        private readonly IOptions<JobSettings> jobSettings;
        
        public JobContext(DbContextOptions<JobContext> options, IOptions<JobSettings> jobSettings)
            : base(options)
        {
            this.jobSettings = jobSettings;
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
