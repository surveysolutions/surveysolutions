using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Scheduler.Services
{
    public interface IJobProgressReporter : IDisposable
    {
        void CompleteJob(long jobId);
        void FailJob(long jobId, Exception exception);
        void UpdateJobData(long jobId, string key, object value);
        void CancelJob(long jobId, string reason);
        Task AbortAsync(CancellationToken cancellationToken);
    }
}
