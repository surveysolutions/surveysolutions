using System;

namespace WB.Services.Scheduler.Services
{
    public interface IJobProgressReporter : IDisposable
    {
        void Start();
        void StartJob(long jobId);
        void CompleteJob(long jobId);
        void FailJob(long jobId, Exception exception);
        void UpdateJobData(long jobId, string key, object value);
        void CancelJob(long jobId, string reason);
    }
}
