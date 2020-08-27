using System.Threading;
using WB.Infrastructure.Native.Monitoring;

namespace WB.UI.Headquarters.Metrics
{
    public class ThreadPoolStatsCollector : IOnDemandCollector
    {
        private readonly Gauge threadPoolQueue = new Gauge(@"wb_threadpool_queue", @"Number of queued items in ThreadPool");
        private readonly Gauge threadPoolCountQueue = new Gauge(@"wb_threadpool_completed_total", @"Total number of completed items in ThreadPool");

        public void RegisterMetrics()
        {
        }

        public void UpdateMetrics()
        {
            threadPoolQueue.Set(ThreadPool.PendingWorkItemCount);
            threadPoolCountQueue.Set(ThreadPool.CompletedWorkItemCount);
        }
    }
}
