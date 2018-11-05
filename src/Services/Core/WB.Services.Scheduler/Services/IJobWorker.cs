using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Scheduler.Services
{
    interface IJobWorker
    {
        Task Task { get; set; }

        Task StartAsync(CancellationToken token);
    }
}
