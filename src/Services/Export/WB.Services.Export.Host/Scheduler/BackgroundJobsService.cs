using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WB.Services.Export.Host.Scheduler
{
    public class BackgroundJobsService : IHostedService
    {
        private readonly IOptions<BackgroundJobsConfig> jobsConfig;
        private BackgroundJobServer server;

        public BackgroundJobsService(IConfiguration configuration, 
            IOptions<BackgroundJobsConfig> jobsConfig,
            IServiceProvider serviceProvider)
        {
            this.jobsConfig = jobsConfig;
            GlobalConfiguration.Configuration.UsePostgreSqlStorage(
                configuration.GetConnectionString("DefaultConnection"),
                new PostgreSqlStorageOptions
                {
                    SchemaName = "scheduler"
                });
            GlobalConfiguration.Configuration.UseColouredConsoleLogProvider();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            server = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                WorkerCount = jobsConfig.Value.JobsCount,
                
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            server?.Dispose();
            return Task.CompletedTask;
        }
    }

    public class HangfireActivator : JobActivator
    {
        private readonly IServiceProvider serviceProvider;

        public HangfireActivator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type type)
        {
            return serviceProvider.GetService(type);
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {

            return base.BeginScope(context);
        }
    } 
}
