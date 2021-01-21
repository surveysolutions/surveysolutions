using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.Services.Infrastructure.Logging;
using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class JobWorker : IJobWorker
    {
        private readonly IServiceProvider serviceProvider;

        private readonly ILogger<JobWorker> logger;
        private static long instanceCounter = 0;
        public long Id { get; set; }
        public Task? Task { get; set; }

        public JobWorker(IServiceProvider serviceProvider,
            ILogger<JobWorker> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            Id = Interlocked.Increment(ref instanceCounter);
        }

        private void Info(string message) => logger.LogInformation("[{Id}] {message}", Id, message);

        public async Task StartAsync(CancellationToken token)
        {
            using var logContext = LoggingHelpers.LogContext("workerId", Environment.MachineName + ":" + this.Id);
            Info("Start new worker");

            while (true)
            {
                try
                {
                    if (token.IsCancellationRequested) return;

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();

                        JobItem? job = await jobService.GetFreeJobAsync(token);
                        if (job != null)
                        {
                            var executor = scope.ServiceProvider.GetRequiredService<IJobExecutor>();
                            await executor.ExecuteAsync(job, token);
                        }
                    }

                    if (token.IsCancellationRequested) return;

                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
                catch (TaskCanceledException)
                { 
                    Info("Cancellation requested. Stopping JobWorker");
                    throw;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "[{Id}] Job worker got an exception. Log, ignore and continue working", Id);
                }
            }
        }
    }
}
