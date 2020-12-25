using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public interface IScheduledTask<TJob, in TJobData> 
        where TJob: IJob<TJobData>
    {
        Task Schedule(TJobData data);
    }
}