using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Infrastructure.Logging;
using WB.Services.Infrastructure.Storage;
using WB.Services.Scheduler.Model;

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
            JobContext db, 
            ILogger<JobExecutor> logger,
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
            using (LoggingHelpers.LogContext(("jobId", job.Id), ("jobTag", job.Tag), ("tenantName", job.TenantName)))
            {
                logger.LogInformation("Start job execution [{tenantName} {jobArgs}]", job.TenantName, job.Args);
                var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(token);

                using var sub = jobCancellation.Subscribe(cancelled =>
                {
                    if (job.Id == cancelled)
                    {
                        logger.LogInformation("Job cancellation requested #{jobId} - {jobTag} {tenantName} [{jobArgs}]",
                            job.Id, job.Tag, job.TenantName, job.Args);
                        linkedCancellation.Cancel(true);
                    }
                });

                try
                {
                    if (SchedulerGlobalConfiguration.JubRunnerHandlers.TryGetValue(job.Type, out var runner))
                    {
                        await using var tr = await db.Database.BeginTransactionAsync(token);

                        var exportJob = serviceProvider.GetService(runner) as IJob;

                        if (exportJob == null)
                        {
                            progressReporter.FailJob(job.Id, new NotImplementedException("Cannot handle job of type: " + job.Type));
                            return;
                        }

                        await exportJob.ExecuteAsync(job.Args, new JobExecutingContext(job), linkedCancellation.Token);
                        progressReporter.CompleteJob(job.Id);
                        await tr.CommitAsync(linkedCancellation.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Job cancelled [ {jobArgs} ]", job.Args);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error during job run [ {jobArgs} ]", job.Args);
                    progressReporter.FailJob(job.Id, e);
                }
            }
        }
    }
}
