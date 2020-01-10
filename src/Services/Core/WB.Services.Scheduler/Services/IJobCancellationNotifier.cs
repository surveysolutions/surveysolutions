using System;
using System.Threading.Tasks;

namespace WB.Services.Scheduler.Services
{
    public interface IJobCancellationNotifier
    {
        Task NotifyOnJobCancellationAsync(long jobId);
        string Channel { get; }
        void JobCancelled(long jobId);
        IDisposable Subscribe(Action<long> action);
    }
}
