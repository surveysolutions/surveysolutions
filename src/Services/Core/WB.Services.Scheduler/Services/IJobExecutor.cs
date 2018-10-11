using System.Threading;
using System.Threading.Tasks;
using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Services
{
    interface IJobExecutor
    {
        Task ExecuteAsync(JobItem job, CancellationToken token);
    }
}
