using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Model;
using WB.Services.Export.Jobs;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services.Implementation
{
    class JobWorker : IJobWorker
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<JobWorker> logger;
        private static long _instanceCounter = 0;
        public long Id { get; set; }
        public Task Task { get; set; }

        public JobWorker(IServiceProvider serviceProvider, ILogger<JobWorker> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            Id = Interlocked.Increment(ref _instanceCounter);
        }

        private void Trace(string message) => logger.LogTrace($"[{Id}] {message}");
        private void Info(string message) => logger.LogInformation($"[{Id}] {message}");

        public async Task StartAsync(CancellationToken token)
        {
            Info("Start new worker");

            while (true)
            {
                token.ThrowIfCancellationRequested();

                using (var scope = serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetService<JobContext>();
                    var jobService = scope.ServiceProvider.GetService<IJobService>();

                    JobItem job;

                    using (var tr = db.Database.BeginTransaction())
                    {
                        job = await jobService.GetFreeJob(token);

                        if (job != null)
                        {
                            await jobService.StartJobAsync(job.Id);
                        }

                        tr.Commit();
                    }

                    if (job != null)
                    {
                        var executor = scope.ServiceProvider.GetService<IJobExecutor>();

                        switch (job.Type)
                        {
                            case JobType.ExportJob:
                                await executor.ExecuteAsync<IExportJob, DataExportProcessArgs>(job,
                                    (export, args, ct) => export.ExecuteAsync(args, ct), token);
                                break;
                            case JobType.Cleanup:
                                await executor.ExecuteAsync<IStaleJobCleanup, bool>(job,
                                    (export, args, ct) => export.ExecuteAsync(args, ct), token);
                                break;
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }
    }
}
