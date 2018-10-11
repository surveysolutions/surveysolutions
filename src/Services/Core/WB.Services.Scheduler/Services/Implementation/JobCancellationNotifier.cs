using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WB.Services.Scheduler.Services.Implementation
{
    class JobCancellationNotifier : IJobCancellationNotifier
    {
        private readonly JobContext db;
        private readonly Subject<long> subject;

        public JobCancellationNotifier(JobContext db)
        {
            this.db = db;
            this.subject = new Subject<long>();
        }

        [SuppressMessage("Possible SQL injection vulnerability", "EF1000")]
        public Task NotifyOnJobCancellationAsync(long jobId)
        {
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
