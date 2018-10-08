using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Model.Events;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class JobProgressReporter : IJobProgressReporter
    {
        private readonly JobContext db;
        private readonly IJobCancellationNotifier jobCancellationNotifier;
        private readonly ILogger<JobProgressReporter> logger;
        private readonly TaskCompletionSource<bool> queueCompletion = new TaskCompletionSource<bool>();

        public JobProgressReporter(JobContext db, IJobCancellationNotifier jobCancellationNotifier, ILogger<JobProgressReporter> logger)
        {
            this.db = db;
            this.jobCancellationNotifier = jobCancellationNotifier;
            this.logger = logger;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                db.ChangeTracker.AutoDetectChangesEnabled = false;
                db.Database.AutoTransactionsEnabled = false;

                foreach (var task in queue.GetConsumingEnumerable())
                {
                    using (var tr = await db.Database.BeginTransactionAsync())
                    {
                        var job = await db.Jobs.Where(j => j.Id == task.Id).SingleOrDefaultAsync();
                        job.Handle(task);
                        db.Jobs.Update(job);

                        if (task is CancelJobEvent)
                        {
                            await jobCancellationNotifier.NotifyOnJobCancellationAsync(job.Id);
                        }

                        await db.SaveChangesAsync();
                        logger.LogTrace(task.ToString());
                        tr.Commit();
                    }
                }

                queueCompletion.SetResult(true);
            });
        }

        public void StartJob(long jobId)
        {
            queue.Add(new StartJobEvent(jobId));
        }

        public void CompleteJob(long jobId)
        {
            queue.Add(new CompleteJobEvent(jobId));
        }

        public void FailJob(long jobId, Exception exception)
        {
            queue.Add(new FailJobEvent(jobId, exception));
        }

        public void UpdateJobData(long jobId, string key, object value)
        {
            queue.Add(new UpdateDataEvent(jobId, key, value));
        }

        public void CancelJob(long jobId, string reason)
        {
            queue.Add(new CancelJobEvent(jobId, reason));
        }

        public Task AbortAsync(CancellationToken cancellationToken)
        {
            queue.CompleteAdding();
            return queueCompletion.Task;
        }

        readonly BlockingCollection<IJobEvent> queue = new BlockingCollection<IJobEvent>();

        public void Dispose()
        {
            queue.CompleteAdding();
        }
    }
}
