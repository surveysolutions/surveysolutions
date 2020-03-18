using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WB.Services.Scheduler.Services.Implementation
{
    class JobCancellationNotifier : IJobCancellationNotifier
    {
        private readonly ILogger<JobCancellationNotifier> logger;
        private readonly Subject<long> subject;
        private readonly IServiceProvider serviceProvider;

        public JobCancellationNotifier(IServiceProvider serviceProvider, ILogger<JobCancellationNotifier> logger)
        {
            this.logger = logger;
            this.subject = new Subject<long>();
            this.serviceProvider = serviceProvider;
        }

        [SuppressMessage("Possible SQL injection vulnerability", "EF1000")]
        public async Task NotifyOnJobCancellationAsync(long jobId)
        {
            using var scope = serviceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<JobContext>();
            logger.LogDebug("Sending request to cancel jobId: {jobId}", jobId);
            string sql = "notify job_cancellation, '" + jobId + "'";
            await db.Database.ExecuteSqlRawAsync(sql);
        }

        public string Channel { get; } = "job_cancellation";

        public void JobCancelled(long jobId)
        {
            this.subject.OnNext(jobId);
        }

        public IDisposable Subscribe(Action<long> action)
        {
            return this.subject.Subscribe(action);
        }
    }
}
