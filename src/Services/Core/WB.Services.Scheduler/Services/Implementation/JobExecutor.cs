using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        private readonly IJobCancellationNotifier jobCancellation;

        public JobExecutor(
            IServiceProvider serviceProvider,
            IJobProgressReporter progressReporter,
            JobContext db, ILogger<JobExecutor> logger,
            IJobCancellationNotifier jobCancellation)
        {
            this.serviceProvider = serviceProvider;
            this.progressReporter = progressReporter;
            this.db = db;
            this.logger = logger;
            this.jobCancellation = jobCancellation;
        }

        public async Task ExecuteAsync(JobItem job, CancellationToken token)
        {
            logger.LogInformation($"Executing job: [{job.Type}] {job.Tenant} {job.Args}");
            var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(token);

            jobCancellation.Subscribe(cancelled =>
            {
                if (job.Id == cancelled)
                {
                    linkedCancellation.Cancel(true);
                }
            });

            try
            {
                if (SchedulerGlobalConfiguration.JubRunnerHandlers.TryGetValue(job.Type, out var runner))
                {
                    using (var tr = await db.Database.BeginTransactionAsync(token))
                    {
                        await db.AcquireXactLockAsync(job.Id);

                        var exportJob = serviceProvider.GetService(runner) as IJob;

                        if (exportJob == null)
                        {
                            progressReporter.FailJob(job.Id,
                                new NotImplementedException("Cannot handle job of type: " + job.Type));
                            return;
                        }

                        await exportJob.ExecuteAsync(job.Args, new JobExecutingContext(job),
                            linkedCancellation.Token);
                        progressReporter.CompleteJob(job.Id);

                        tr.Commit();
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                logger.LogWarning($"Job cancelled: [{job.Type}] {job.Tenant} {job.Args}");
                progressReporter.CancelJob(job.Id, oce.Message);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error during job run: [{job.Type}] {job.Tenant} {job.Args}");
                progressReporter.FailJob(job.Id, e);
            }

        }
    }
}
