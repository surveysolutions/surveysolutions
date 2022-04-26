using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Model.Events;

namespace WB.Services.Scheduler.Services.Implementation
{
    class JobProgressReportWriter : IJobProgressReportWriter
    {
        private readonly JobContext jobContext;
        private readonly IJobCancellationNotifier jobCancellationNotifier;
        private readonly ILogger<JobProgressReportWriter> logger;

        public JobProgressReportWriter(JobContext jobContext, IJobCancellationNotifier jobCancellationNotifier, 
            ILogger<JobProgressReportWriter> logger)
        {
            this.jobContext = jobContext;
            this.jobCancellationNotifier = jobCancellationNotifier;
            this.logger = logger;
        }

        public async Task WriteReportAsync(IJobEvent task, CancellationToken stoppingToken)
        {
            var db = this.jobContext;

            await using var tr = await db.Database.BeginTransactionAsync(stoppingToken);
            var job = await db.Jobs.FindAsync(task.Id);
            if (job == null)
            {
                return;
            }

            job.Handle(task);
            db.Jobs.Update(job);

            if (task is CancelJobEvent)
            {
                await jobCancellationNotifier.NotifyOnJobCancellationAsync(job.Id);
            }

            await db.SaveChangesAsync(stoppingToken);
            logger.LogTrace(task.ToString());
            await tr.CommitAsync(stoppingToken);
        }
    }
}
