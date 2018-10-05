using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class JobWorker : IJobWorker
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<JobWorker> logger;
        private static long _instanceCounter = 0;
        public long Id { get; set; }
        public Task Task { get; set; }

        public JobWorker(IServiceProvider serviceProvider,
            ILogger<JobWorker> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            Id = Interlocked.Increment(ref _instanceCounter);
        }

        //private void Trace(string message) => logger.LogTrace($"[{Id}] {message}");
        private void Info(string message) => logger.LogInformation($"[{Id}] {message}");

        public async Task StartAsync(CancellationToken token)
        {
            Info("Start new worker");
            
            while (true)
            {
                if (token.IsCancellationRequested) return;

                using (var scope = serviceProvider.CreateScope())
                {
                    var jobService = scope.ServiceProvider.GetService<IJobService>();

                    JobItem job = await jobService.GetFreeJobAsync(token);

                    if (job != null)
                    {
                        var executor = scope.ServiceProvider.GetService<IJobExecutor>();

                        await executor.ExecuteAsync(job, token);
                    }
                }

                if (token.IsCancellationRequested) return;

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }
    }
}
