using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services
{
    interface IJobWorker
    {
        Task Task { get; set; }

        Task StartAsync(CancellationToken token);
    }
}
