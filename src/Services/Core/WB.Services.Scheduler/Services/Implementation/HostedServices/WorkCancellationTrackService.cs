using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Infrastructure.Logging;

namespace WB.Services.Scheduler.Services.Implementation.HostedServices
{
    internal class WorkCancellationTrackService : IHostedSchedulerService
    {
        private readonly IConfiguration configuration;
        private readonly IOptions<JobSettings> options;
        private readonly IJobCancellationNotifier cancellationNotification;
        private readonly ILogger<WorkCancellationTrackService> logger;

        public WorkCancellationTrackService(
            IConfiguration configuration, 
            IOptions<JobSettings> options, 
            IJobCancellationNotifier cancellationNotification,
            ILogger<WorkCancellationTrackService> logger )
        {
            this.configuration = configuration;
            this.options = options;
            this.cancellationNotification = cancellationNotification;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                using var ctx = LoggingHelpers.LogContext("workerId", "CancellationService");
                while (!cancellationToken.IsCancellationRequested)
                {
                    var connectionString = configuration.GetConnectionString(options.Value.ConnectionName);

                    await using var connection = new NpgsqlConnection(connectionString);
                    await connection.OpenAsync(cancellationToken);
                        
                    connection.Notification += (conn, arg) =>
                    {
                        if (long.TryParse(arg.Payload, out var jobId))
                        {
                            logger.LogDebug("Job cancellation acquired from DB. JobId: {jobId}", jobId);
                            cancellationNotification.JobCancelled(jobId);
                        }
                    };
                        
                    await connection.ExecuteAsync($"LISTEN {cancellationNotification.Channel}");

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await connection.WaitAsync(cancellationToken);
                    }
                }
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
