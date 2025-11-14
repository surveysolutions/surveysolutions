using System.Diagnostics;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Infrastructure.Storage;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Model.Mapping;

namespace WB.Services.Scheduler
{
    [DebuggerDisplay("db #{Id}")]
    public class JobContext : DbContext
    {
        private static long counter = 0;
        public long Id { get; }
        private readonly IOptions<JobSettings> jobSettings;
        private readonly ILoggerFactory? loggerFactory;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public JobContext(DbContextOptions<JobContext> options,
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            IOptions<JobSettings> jobSettings,
            ILoggerFactory? loggerFactory = null)
            : base(options)
        {
            this.jobSettings = jobSettings;
            this.loggerFactory = loggerFactory;
            Id = Interlocked.Increment(ref counter);
        }

        public DbSet<JobItem> Jobs { get; set; }
        public DbSet<JobArchive> Archive { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSnakeCaseNaming();
            modelBuilder.HasDefaultSchema(jobSettings.Value.SchemaName);
            modelBuilder.ApplyConfiguration(new JobItemConfiguration());
            modelBuilder.ApplyConfiguration(new JobArchiveConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (loggerFactory != null)
            {
                optionsBuilder.UseLoggerFactory(loggerFactory);
            }

            base.OnConfiguring(optionsBuilder);
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
            optionsBuilder.UseNpgsql("...");

            return new JobContext(optionsBuilder.Options, Options.Create(new JobSettings
            {
                SchemaName = "scheduler"
            }));
        }
    }
}
