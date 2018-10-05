using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Storage;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class JobExecutor : IJobExecutor
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IJobProgressReporter progressReporter;
        private readonly JobContext db;
        private readonly ILogger<JobExecutor> logger;

        public JobExecutor(
            IServiceProvider serviceProvider,
            IJobProgressReporter progressReporter,
            JobContext db, ILogger<JobExecutor> logger)
        {
            this.serviceProvider = serviceProvider;
            this.progressReporter = progressReporter;
            this.db = db;
            this.logger = logger;
        }

        public async Task ExecuteAsync(JobItem job, CancellationToken token)
        {
            logger.LogInformation($"Executing job: [{job.Type}] {job.Tenant} {job.Args}");

            try
            {
                if (SchedulerGlobalConfiguration.JubRunnerHandlers.TryGetValue(job.Type, out var runner))
                {
                    await db.TryAcquireLockAsync(job.Id);
                    var exportJob = serviceProvider.GetService(runner) as IJob;

                    if (exportJob == null)
                    {
                        progressReporter.FailJob(job.Id, new NotImplementedException("Cannot handle job of type: " + job.Type.ToString()));
                        return;
                    }

                    await exportJob.ExecuteAsync(job.Args, token);
                    progressReporter.CompleteJob(job.Id);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error during job run: [{job.Type}] {job.Tenant} {job.Args}");
                progressReporter.FailJob(job.Id, e);
            }
        }

    }
}
