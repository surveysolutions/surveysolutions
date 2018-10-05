using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Model.Events;
using WB.Services.Scheduler.Storage;

namespace WB.Services.Scheduler.Services.Implementation
{
    internal class JobProgressReporter : IDisposable, IJobProgressReporter
    {
        private readonly JobContext db;
        private readonly ILogger<JobProgressReporter> logger;

        public JobProgressReporter(JobContext db, ILogger<JobProgressReporter> logger)
        {
            this.db = db;
            this.logger = logger;
            this.cts = new CancellationTokenSource();
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                foreach (var task in queue.GetConsumingEnumerable(cts.Token))
                {
                    using (var tr = await db.Database.BeginTransactionAsync(cts.Token))
                    {
                        var job = await db.Jobs.Where(j => j.Id == task.Id).SingleOrDefaultAsync();
                        job.Handle(task);
                        db.Jobs.Update(job);
                        await db.SaveChangesAsync(cts.Token);
                        logger.LogTrace(task.ToString());
                        tr.Commit();
                    }
                }
            }, cts.Token);
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

        readonly BlockingCollection<IJobEvent> queue = new BlockingCollection<IJobEvent>();

        private readonly CancellationTokenSource cts;
        
        public void Dispose()
        {
            queue.CompleteAdding();

            cts.Cancel();
            cts.Dispose();
        }
    }
}
