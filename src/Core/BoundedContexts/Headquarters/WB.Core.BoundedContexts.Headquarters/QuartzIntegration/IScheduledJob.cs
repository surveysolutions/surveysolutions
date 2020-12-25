using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public interface IScheduledJob
    {
        Task RegisterJob();
    }
}