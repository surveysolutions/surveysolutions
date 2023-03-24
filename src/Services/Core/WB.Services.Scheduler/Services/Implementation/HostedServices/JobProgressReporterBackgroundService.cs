using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Services.Infrastructure.Logging;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Model.Events;

namespace WB.Services.Scheduler.Services.Implementation.HostedServices
{
    internal class JobProgressReporterBackgroundService : BackgroundService, IJobProgressReporter
    {
        private readonly ILogger<JobProgressReporterBackgroundService> logger;
        private readonly TaskCompletionSource<bool> queueCompletion = new TaskCompletionSource<bool>();

        public JobProgressReporterBackgroundService(IServiceProvider serviceProvider, 
            ILogger<JobProgressReporterBackgroundService> logger)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                using var ctx = LoggingHelpers.LogContext("workerId", "progressReporter");
                logger.LogInformation("{service} started", nameof(JobProgressReporterBackgroundService));
                foreach (var task in queue.GetConsumingEnumerable())
                {
                    try
                    {
                        using var scope = serviceProvider.CreateScope();
                        var runner = scope.ServiceProvider.GetRequiredService<IJobProgressReportWriter>();
                        stoppingToken.ThrowIfCancellationRequested();
                        await runner.WriteReportAsync(task, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogError("{service} cancellation requested. Stopping", nameof(JobProgressReporterBackgroundService));
                        return;
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception during progress reporting for jobId: {task.Id}", e);
                    }
                }

                queueCompletion.SetResult(true);
            }, stoppingToken);
        }

        public void CompleteJob(long jobId)
        {
            if (!queue.IsAddingCompleted)
                queue.Add(new CompleteJobEvent(jobId));
        }

        public void FailJob(long jobId, Exception exception)
        {
            if (!queue.IsAddingCompleted)
                queue.Add(new FailJobEvent(jobId, exception));
        }

        public void UpdateJobData(long jobId, string key, object value)
        {
            if (!queue.IsAddingCompleted)
                queue.Add(new UpdateDataEvent(jobId, key, value));
        }

        public void CancelJob(long jobId, string reason)
        {
            if (!queue.IsAddingCompleted)
                queue.Add(new CancelJobEvent(jobId, reason));
        }

        public Task AbortAsync(CancellationToken cancellationToken)
        {
            queue.CompleteAdding();
            queueCompletion.Task.Wait(TimeSpan.FromSeconds(1)); // waiting at least 1 second to complete queue
            return Task.CompletedTask;
        }

        readonly BlockingCollection<IJobEvent> queue = new BlockingCollection<IJobEvent>();
        private readonly IServiceProvider serviceProvider;

        public override void Dispose()
        {
            if (!queue.IsCompleted)
                queue.CompleteAdding();
            
            base.Dispose();
        }
    }
}
