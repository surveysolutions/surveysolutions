using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public interface IScheduledTask<TJob, in TJobData> 
        where TJob: IJobWithData<TJobData>
    {
        Task Schedule(TJobData data);
        Task<bool> IsJobRunning(CancellationToken token = default);
    }
}
