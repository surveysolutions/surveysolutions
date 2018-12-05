using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WB.Services.Scheduler.Services.Implementation
{
    class JobCancellationNotifier : IJobCancellationNotifier
    {
        private readonly JobContext db;
        private readonly ILogger<JobCancellationNotifier> logger;
        private readonly Subject<long> subject;

        public JobCancellationNotifier(JobContext db, ILogger<JobCancellationNotifier> logger)
        {
            this.db = db;
            this.logger = logger;
            this.subject = new Subject<long>();
        }

        [SuppressMessage("Possible SQL injection vulnerability", "EF1000")]
        public Task NotifyOnJobCancellationAsync(long jobId)
        {
            logger.LogDebug("Sending request to cancel jobId: {jobId}", jobId);
            string sql = "notify job_cancellation, '" + jobId + "'";
            return db.Database.ExecuteSqlCommandAsync(sql);
        }

        public string Channel { get; } = "job_cancellation";

        public void JobCancelled(long jobId)
        {
            this.subject.OnNext(jobId);
        }

        public void Subscribe(Action<long> action)
        {
            this.subject.Subscribe(action);
        }
    }
}
