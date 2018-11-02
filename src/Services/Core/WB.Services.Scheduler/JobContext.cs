using System.Diagnostics;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Storage;

namespace WB.Services.Scheduler
{
    [DebuggerDisplay("db #{Id}")]
    public class JobContext : DbContext
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

    /// <summary>
    /// This class only needed for local development. It's not used in runtime.
    /// This class required for `Add-Migration` to work from Package Manager Console
    /// </summary>
    public class JobContextFactory : IDesignTimeDbContextFactory<JobContext>
    {
        public JobContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<JobContext>();
            optionsBuilder.UseNpgsql(
                "Server=127.0.0.1;Port=5432;User Id=postgres;Password=Qwerty1234;Database=ExportService;");

            return new JobContext(optionsBuilder.Options, Options.Create(new JobSettings
            {
                SchemaName = "scheduler"
            }));
        }
    }
}
