using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using WB.Services.Export.Checks;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services.Implementation;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue
{
    public static class JobServiceExtensions
    {
        public static void UseJobService(this IServiceCollection services, IConfiguration configuration, string section = "job")
        {
            services.AddTransient<IHealthCheck, DbHealthCheck>();
            services.AddTransient<IJobService, JobService>();
            services.AddSingleton<IJobProgressReporter, JobProgressReporter>();
            services.AddTransient<IJobWorker, JobWorker>();
            services.AddTransient<IJobExecutor, JobExecutor>();
            services.AddTransient<IStaleJobCleanup, StaleJobCleanup>();
            services.AddDbContext<JobContext>(ops =>
                ops
                    .UseLoggerFactory(MyLoggerFactory)
                    .UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.Configure<JobSettings>(configuration.GetSection(section));
        }

        public static readonly LoggerFactory MyLoggerFactory
            = new LoggerFactory(new[] { new ConsoleLoggerProvider((s, level) => (int) level >= 3, true ) });
    }
}
