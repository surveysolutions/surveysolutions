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

        public void StartProgressReporter()
        {
            Task.Factory.StartNew(async () =>
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
            }, TaskCreationOptions.LongRunning);
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
            if(!queue.IsAddingCompleted)
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
            queueCompletion.Task.Wait(TimeSpan.FromSeconds(5)); // waiting at least 5 seconds to complete queue
            return Task.CompletedTask;
        }

        readonly BlockingCollection<IJobEvent> queue = new BlockingCollection<IJobEvent>();

        public void Dispose()
        {
            if(!queue.IsCompleted)
                queue.CompleteAdding();
        }
    }
}
